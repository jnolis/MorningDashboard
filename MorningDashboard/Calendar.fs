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
    type CustomFunctions = {
        BusyStatus: EWSoftware.PDI.Objects.VEvent -> BusyStatus
        IsAllDay: EWSoftware.PDI.Objects.VEvent -> bool
        }

    type CalendarInfo = {Name: string; Url: string; Type: CalendarType}
    type Instance = {Name: string; StartTime: System.DateTimeOffset; EndTime: System.DateTimeOffset; BusyStatus: BusyStatus; IsAllDay: bool}
    type Calendar = {Name: string; StartRange: System.DateTimeOffset; EndRange: System.DateTimeOffset; Instances: Instance seq}

    let calendarCache = SharedCode.makeNewCache<CalendarInfo*System.DateTimeOffset*System.DateTimeOffset,Calendar>()


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
            {CustomFunctions.BusyStatus = busyStatus; IsAllDay = isAllDay}

        let googleFunctions = 
            let busyStatus (event: EWSoftware.PDI.Objects.VEvent) =
                try match event.Status.Value with
                    | "CONFIRMED" -> Busy
                    | "TENTATIVE" -> Tentative
                    | "CANCELLED" -> Free
                    | _ -> Busy
                with | _ -> Busy
            let isAllDay (event: EWSoftware.PDI.Objects.VEvent) = false
            {CustomFunctions.BusyStatus = busyStatus; IsAllDay = isAllDay}
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

    let getInstancesInRange (customFunctions: CustomFunctions) (calendarName: string) (calendar: EWSoftware.PDI.Objects.VCalendar) (startDate: System.DateTimeOffset) (endDate: System.DateTimeOffset) =
        calendar.Events 
        |> Seq.map (fun e -> 
            let eventName = e.Summary.Value
            let busyStatus = customFunctions.BusyStatus e
            let isAllDay = customFunctions.IsAllDay e
            e.InstancesBetween(startDate.LocalDateTime,endDate.LocalDateTime,true)
            |> Seq.map (fun instance -> 
                            let startTime = System.DateTimeOffset instance.StartDateTime
                            let endTime = System.DateTimeOffset instance.EndDateTime
                            {Name= eventName; StartTime = startTime; EndTime = endTime; BusyStatus = busyStatus; IsAllDay = isAllDay}
                            )
            |> Seq.filter (fun instance -> instance.StartTime < endDate && instance.EndTime > startDate)
            )
        |> Seq.concat
        |> Seq.sortBy (fun instance -> (instance.StartTime, instance.Name))

    let getCalendar (startRange: System.DateTimeOffset) (endRange: System.DateTimeOffset) (calendarInfo:CalendarInfo) =
        try
            let customFunctions = getCustomFunctions calendarInfo.Type
            let rawCalendar = getRawCalendar calendarInfo.Url
            let instances = getInstancesInRange customFunctions calendarInfo.Name rawCalendar startRange endRange
            Some {Name = calendarInfo.Name; Instances = instances; StartRange = startRange; EndRange = endRange}
        with | _ -> None

    let getCalendarWithCache (startRange: System.DateTimeOffset) (endRange: System.DateTimeOffset) (calendarInfo) =
        SharedCode.getFromCache calendarCache (15.0*60.0-5.0) (fun (i,s,e) -> getCalendar s e i) (calendarInfo,startRange,endRange)
        
    let getAllCalendars (startRange: System.DateTimeOffset) (endRange: System.DateTimeOffset) =
        getCalendarInfo (SharedCode.getKeyFile ())
            |> Seq.map (getCalendar startRange endRange)

    let getAllCalendarsWithCache (startRange: System.DateTimeOffset) (endRange: System.DateTimeOffset) =
        getCalendarInfo (SharedCode.getKeyFile ())
            |> Seq.map (getCalendarWithCache startRange endRange)