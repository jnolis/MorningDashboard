namespace MorningDashboard

open Newtonsoft.Json

module Calendar =
    type CalendarType =
        | Outlook
        | Google
    type BusyStatus =
        | Free
        | Tentative
        | Busy
        | OutOfOffice


    type CalendarInfo = {Name: string; Url: string; Type: CalendarType}
    type Instance = {
        Domain: string; 
        Name: string; 
        StartTime: System.DateTimeOffset; 
        EndTime: System.DateTimeOffset; 
        BusyStatus: BusyStatus; 
        IsAllDay: bool}

    type CustomFunctions = {
        BusyStatus: EWSoftware.PDI.Objects.VEvent -> BusyStatus
        IsAllDay: EWSoftware.PDI.Objects.VEvent -> bool
        InstanceFixer: Instance -> Instance
        }
    type Calendar = {Name: string; Date: System.DateTime; Instances: Instance seq}
    let calendarCache = SharedCode.makeNewCache<CalendarInfo*System.DateTime,Calendar>()

    let cleanCache () =
        let today = System.DateTime.Today
        calendarCache.Keys
        |> Seq.filter (fun x -> snd x < today)
        |> Seq.iter (calendarCache.Remove >> ignore)

    let getCustomFunctions (t: CalendarType) =
        let outlookFunctions = 
            let busyStatus (event: EWSoftware.PDI.Objects.VEvent) =
                try match event.CustomProperties.["X-MICROSOFT-CDO-BUSYSTATUS"].Value with
                    | "BUSY" -> Busy
                    | "TENTATIVE" -> Tentative
                    | "FREE" -> Free
                    | "OOF" -> OutOfOffice
                    | _ -> Busy
                with | _ -> Busy
            let isAllDay (event: EWSoftware.PDI.Objects.VEvent) =
                try event.CustomProperties.["X-MICROSOFT-CDO-ALLDAYEVENT"].Value = "TRUE"
                with | _ -> false

            let instanceFixer (instance:Instance) =
                let checkIfInvitation i =
                    new System.Text.RegularExpressions.Regex(@"^Invitation: (.*) @ .+ \(.*\)")
                    |> (fun x -> x.Match i)
                    |> (fun x -> if x.Groups.[1].Success then (true,x.Groups.[1].Value) else (false,i))
                let (isGoogleInvitation,updatedName) = checkIfInvitation instance.Name
                let isMidnight (dt:System.DateTime) =
                    dt.Hour = 0 && dt.Minute = 0 && dt.Second = 0 && dt.Millisecond = 0
                if isGoogleInvitation then
                    if (not instance.IsAllDay) && isMidnight instance.StartTime.UtcDateTime && isMidnight instance.EndTime.UtcDateTime then
                        let newStartTime = System.DateTimeOffset (System.DateTime.SpecifyKind(instance.StartTime.UtcDateTime.Date,System.DateTimeKind.Local))
                        let newEndTime = System.DateTimeOffset (System.DateTime.SpecifyKind(instance.EndTime.UtcDateTime.Date,System.DateTimeKind.Local))
                        let newIsAllDay = true
                        {instance with 
                            StartTime = newStartTime;
                            EndTime = newEndTime;
                            IsAllDay = newIsAllDay;
                            Name = updatedName}
                    else {instance with Name = updatedName}
                else instance
            {CustomFunctions.BusyStatus = busyStatus; IsAllDay = isAllDay; InstanceFixer = instanceFixer}

        let googleFunctions = 
            let busyStatus (event: EWSoftware.PDI.Objects.VEvent) =
                try match event.Status.Value with
                    | "CONFIRMED" -> Busy
                    | "TENTATIVE" -> Tentative
                    | "CANCELLED" -> Free
                    | _ -> Busy
                with | _ -> Busy
            let isAllDay (event: EWSoftware.PDI.Objects.VEvent) = false
            {CustomFunctions.BusyStatus = busyStatus; IsAllDay = isAllDay; InstanceFixer = id}
        match t with
        | Outlook -> outlookFunctions
        | Google -> googleFunctions

    let getCalendarInfo (keyFile: string) =
        let getType (s:string) = 
            match s with | "Outlook" -> Outlook | _ -> Google
        keyFile
        |> System.IO.File.ReadAllText
        |> Linq.JObject.Parse
        |> (fun x -> x.["Calendars"])
        |> Seq.map (fun calendar -> {Name = string calendar.["Name"]; Url = string calendar.["Url"]; Type = getType (string calendar.["Type"])})

    let getRawCalendar (calendarUrl: string) =
        let parser = new EWSoftware.PDI.Parser.VCalendarParser()
        do calendarUrl
        |> ((new System.Net.WebClient()).DownloadString)
        |> parser.ParseString
        parser.VCalendar

    let getInstancesInRange (customFunctions: CustomFunctions) (calendarName: string) (calendar: EWSoftware.PDI.Objects.VCalendar) (date: System.DateTime) =
        let dateStartTime = date.Date
        let dateEndTime = date.Date.AddDays(1.0)
        calendar.Events 
        |> Seq.map (fun e -> 
            let eventName = e.Summary.Value
            let busyStatus = customFunctions.BusyStatus e
            let isFlaggedAllDay = customFunctions.IsAllDay e
            e.InstancesBetween(dateStartTime,dateEndTime,true)
            |> Seq.map (fun instance -> 
                            let startTime =  instance.StartDateTime
                            let endTime = instance.EndDateTime
                            let isAllDay = isFlaggedAllDay || (startTime <= dateStartTime  && endTime >= dateEndTime)
                            {Domain= calendarName; 
                            Name= eventName; 
                            StartTime = System.DateTimeOffset startTime; 
                            EndTime = System.DateTimeOffset endTime; 
                            BusyStatus = busyStatus;
                            IsAllDay = isAllDay}
                            )
            |> Seq.map customFunctions.InstanceFixer
            |> Seq.filter (fun instance -> instance.StartTime < System.DateTimeOffset dateEndTime && instance.EndTime > System.DateTimeOffset dateStartTime)
            )
        |> Seq.concat
        |> Seq.sortBy (fun instance -> (instance.StartTime, instance.Name))

    let getCalendar (date: System.DateTime) (calendarInfo:CalendarInfo) =
        try
            let customFunctions = getCustomFunctions calendarInfo.Type
            let rawCalendar = getRawCalendar calendarInfo.Url
            let instances = getInstancesInRange customFunctions calendarInfo.Name rawCalendar date
            Some {Calendar.Name = calendarInfo.Name;Instances = instances;Date=date}
        with | _ -> None

    let getCalendarWithCache (date: System.DateTime) (calendarInfo) =
        cleanCache()
        SharedCode.getFromCache calendarCache (15.0*60.0-5.0) (fun (i,d) -> getCalendar d i) (calendarInfo,date)

    let getCombinedCalendarWithCache (date: System.DateTime) (calendarInfos: CalendarInfo seq) =
        let calendarInstanceSets = 
            calendarInfos
            |> Seq.map (fun cal -> async { return getCalendarWithCache date cal })
            |> Async.Parallel
            |> Async.RunSynchronously
            |> Seq.choose id

        let instances =
            calendarInstanceSets
            |> Seq.map (fun calendar -> calendar.Instances)
            |> Seq.concat
            |> Seq.groupBy (fun instance -> (instance.StartTime,instance.EndTime,instance.Name,instance.IsAllDay))
            |> Seq.map (fun (key, s) ->
                let domain =
                    s 
                    |> Seq.map (fun instance -> instance.Domain)
                    |> Seq.sort
                    |> SharedCode.formatter
                {(Seq.head s) with Domain = domain})
        let date = (Seq.head calendarInstanceSets  ).Date
        let name = calendarInstanceSets |> Seq.map (fun x -> x.Name) |> Seq.sort |> SharedCode.formatter
        {Name = name; Date = date; Instances = instances}