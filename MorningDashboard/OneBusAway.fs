namespace MorningDashboard

open Newtonsoft.Json

module OneBusAway =
    let apiKey = SharedCode.getKeyFromProject "OneBusAway"

    
    type CommuteId = {DepartureStopId: string; RouteIds: string seq; ArrivalStopId: string}
    type TripType = | Scheduled | Predicted
    type Commute = 
        {
            Name: string; 
            Departure: System.DateTimeOffset; 
            Arrival: System.DateTimeOffset option;
            Type: TripType}
    type Route = {Id: string; LongName: string; ShortName: string; Description: string}
    type Stop = {Id: string; Name: string; Direction: string}
    type Trip = {Id: string}
    let routeCache =SharedCode.makeNewCache<string,Route>()
    let stopCache = SharedCode.makeNewCache<string,Stop>()
    let commuteCache = SharedCode.makeNewCache<Stop*(Route seq)*(Stop option),Commute seq>()

    let getArrivalTimeForTripAndStop (trip:Trip) (arrivalStop: Stop) =
        try
            let json =
                @"http://api.pugetsound.onebusaway.org/api/where/trip-details/"+trip.Id+".json?key="+apiKey
                |> ((new System.Net.WebClient()).DownloadString)
                |> Linq.JObject.Parse
            let serviceDate = 
                json.["data"].["entry"].["serviceDate"] 
                |> int64 
                |> System.DateTimeOffset.FromUnixTimeMilliseconds
            let convertArrivalTime (serviceDate:System.DateTimeOffset) (arrivalTime:float) =
                serviceDate.AddSeconds arrivalTime
            let stopTimes =
                json.["data"].["entry"].["schedule"].["stopTimes"]
                |> Seq.filter (fun st -> string st.["stopId"] = arrivalStop.Id)
                |> Seq.map (fun st -> float st.["arrivalTime"])
                |> Seq.map (convertArrivalTime serviceDate)
            if Seq.length stopTimes > 0 then
                Some (Seq.min stopTimes)
            else None
        with | _ -> None

    let getCommutesForStopAndRoutes (departureStop: Stop) (routes:Route seq) (arrivalStop: Stop option) : Commute seq=
        let json =
            @"http://api.pugetsound.onebusaway.org/api/where/arrivals-and-departures-for-stop/"+departureStop.Id+".json?key="+apiKey
            |> ((new System.Net.WebClient()).DownloadString)
            |> Linq.JObject.Parse
        if string json.["code"] <> "200" then Seq.empty<Commute>
        else
            let currentTimeReference = json.["currentTime"]
                                        |> int64
                                        |> System.DateTimeOffset.FromUnixTimeMilliseconds
                                        |> (fun x -> System.TimeZoneInfo.ConvertTime(x,System.TimeZoneInfo.Local))
            let currentTime = System.DateTimeOffset.Now                    
            json
            |> (fun data -> data.["data"].["entry"].["arrivalsAndDepartures"])
            |> Seq.choose (fun arrivalData -> 
                            match Seq.tryFind (fun (r:Route) -> r.Id = (string arrivalData.["routeId"])) routes with
                            | Some route ->
                                let isPredicted = 
                                    match string arrivalData.["predicted"] with
                                    | "True" -> true
                                    | "False" -> false
                                    | _ -> failwith "Invalid data"
                                let scheduledDepartureInt = 
                                    arrivalData.["scheduledDepartureTime"] 
                                    |> int64 
                                let scheduledDeparture =
                                    scheduledDepartureInt
                                    |> System.DateTimeOffset.FromUnixTimeMilliseconds
                                    |> (fun x -> System.TimeZoneInfo.ConvertTime(x,System.TimeZoneInfo.Local))
                                    |> SharedCode.adjustToSystemTime currentTimeReference
                                let predictedDeparture = 
                                    let predictedInt = arrivalData.["predictedDepartureTime"] 
                                                        |> int64 
                                    if predictedInt > 0L && isPredicted then
                                        predictedInt
                                        |> System.DateTimeOffset.FromUnixTimeMilliseconds
                                        |> (fun x -> System.TimeZoneInfo.ConvertTime(x,System.TimeZoneInfo.Local))
                                        |> SharedCode.adjustToSystemTime currentTimeReference
                                        |> Some
                                    else None
                                let trip = {Id = (string arrivalData.["tripId"])}
                                if scheduledDepartureInt = 0L then None else
                                        Some {
                                                Commute.Name= route.ShortName; 
                                                Commute.Departure = match predictedDeparture with 
                                                                    | Some p -> p 
                                                                    | None -> scheduledDeparture;
                                                Commute.Arrival = match arrivalStop with
                                                                    | Some a -> getArrivalTimeForTripAndStop trip a
                                                                    | None -> None;
                                                Commute.Type= if isPredicted then Predicted else Scheduled}
                            | None -> None
                                    )
        
    let getCommutesForStopAndRoutesWithCache (departureStop: Stop) (routes:Route seq) (arrivalStop: Stop option) =
        let arrivals = SharedCode.getFromCache commuteCache (14.5) (fun (d,r,a) -> getCommutesForStopAndRoutes d r a |> Some) (departureStop,routes,arrivalStop)
        match arrivals with
        | Some a -> a
        | None -> Seq.empty<Commute>

    let getRouteInfo (routeId:string) : Route option =
        let json = 
            @"http://api.pugetsound.onebusaway.org/api/where/route/"+routeId+".json?key="+apiKey
            |> ((new System.Net.WebClient()).DownloadString)
            |> Linq.JObject.Parse
        if string json.["code"] <> "200" then None
        else
            let data = json.["data"].["entry"]
            let id = routeId
            let shortName = string data.["shortName"]
            let longName = string data.["longName"]
            let description = string data.["description"]
            Some {Id = id; LongName = longName; ShortName = shortName; Description = description}

    let getRouteInfoWithCache (routeId:string) : Route option =
        SharedCode.getFromCache routeCache (60.0*24.0) getRouteInfo routeId

    let getStopInfo (stopId:string) : Stop option =
        let json = @"http://api.pugetsound.onebusaway.org/api/where/stop/"+stopId+".json?key="+apiKey
                    |> ((new System.Net.WebClient()).DownloadString)
                    |> Linq.JObject.Parse
        if string json.["code"] <> "200" then None
        else
            let data = json.["data"].["entry"]
            let id = stopId
            let name = string data.["name"]
            let direction = string data.["direction"]
            Some {Id = id; Name = name; Direction = direction}

    let getStopInfoWithCache (stopId:string) : Stop option =
        SharedCode.getFromCache stopCache (60.0*24.0) getStopInfo stopId