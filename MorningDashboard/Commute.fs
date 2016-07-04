namespace MorningDashboard

open Newtonsoft.Json

module Commute =
    
    type CarParameters = 
        {Origin: string;
         Destination: string}
    type WalkParameters = {WalkingHours: int; WalkingMinutes: int; WalkingSeconds: int}
    type BusParameters = {DepartureStopId: string; RouteIds: string seq; ArrivalStopId: string}
    type CommuteParameters =
            | Car of CarParameters
            | Walk of WalkParameters
            | Bus of BusParameters
            | Transfer of CommuteParameters seq
            | Option of CommuteParameters seq

    type TripMode =
        | Car
        | BusScheduled of string
        | BusPredicted of string
        | Walk
        


    type Trip = {Departure: System.DateTimeOffset; Duration: System.TimeSpan; Mode: TripMode seq}




    let walkParametersToTrips (w:WalkParameters) (minDepartureTime: System.DateTimeOffset) =
        Seq.singleton
            {
            Mode = Seq.singleton Walk;
            Departure = minDepartureTime;
            Duration = new System.TimeSpan(w.WalkingHours,w.WalkingMinutes,w.WalkingSeconds)
            }
    module OneBusAway =
        let apiKey = SharedCode.getKeyFromProject "OneBusAway"

    
        type BusRoute = {Id: string; LongName: string; ShortName: string; Description: string}
        type BusStop = {Id: string; Name: string; Direction: string}
        type BusTrip = {Id: string}
        let routeCache =SharedCode.makeNewCache<string,BusRoute>()
        let stopCache = SharedCode.makeNewCache<string,BusStop>()
        let tripCache = SharedCode.makeNewCache<BusStop*(BusRoute seq)*(BusStop),Trip seq>()

        let getArrivalTimeForTripAndStop (trip:BusTrip) (arrivalStop: BusStop) =
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

        let getTrips (departureStop: BusStop) (routes:BusRoute seq) (arrivalStop: BusStop) : Trip seq=
            let json =
                @"http://api.pugetsound.onebusaway.org/api/where/arrivals-and-departures-for-stop/"+departureStop.Id+".json?key="+apiKey
                |> ((new System.Net.WebClient()).DownloadString)
                |> Linq.JObject.Parse
            if string json.["code"] <> "200" then Seq.empty<Trip>
            else
                let currentTimeReference = json.["currentTime"]
                                            |> int64
                                            |> System.DateTimeOffset.FromUnixTimeMilliseconds
                                            |> (fun x -> System.TimeZoneInfo.ConvertTime(x,System.TimeZoneInfo.Local))
                let currentTime = System.DateTimeOffset.Now                    
                json
                |> (fun data -> data.["data"].["entry"].["arrivalsAndDepartures"])
                |> Seq.map (fun arrivalData -> 
                                async {
                                        let result = 
                                            match Seq.tryFind (fun (r:BusRoute) -> r.Id = (string arrivalData.["routeId"])) routes with
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
                                                let arrivalTimeOption = getArrivalTimeForTripAndStop trip arrivalStop
                                                if scheduledDepartureInt = 0L || arrivalTimeOption.IsNone then None else
                                                    match predictedDeparture with
                                                    | Some p ->
                                                        Some {
                                                                Trip.Mode = Seq.singleton(BusPredicted route.ShortName); 
                                                                Trip.Departure = p;
                                                                Trip.Duration = arrivalTimeOption.Value - p}
                                                    | None ->
                                                        Some {
                                                                Trip.Mode = Seq.singleton(BusScheduled route.ShortName); 
                                                                Trip.Departure = scheduledDeparture;
                                                                Trip.Duration = arrivalTimeOption.Value - scheduledDeparture}
                                            | None -> None
                                        return result
                                        }
                                        )
                |> Async.Parallel
                |> Async.RunSynchronously
                |> Seq.ofArray
                |> Seq.choose id
        
        let getTripsWithCache (departureStop: BusStop) (routes:BusRoute seq) (arrivalStop: BusStop) =
            let arrivals = SharedCode.getFromCache tripCache (14.5) (fun (d,r,a) -> getTrips d r a |> Some) (departureStop,routes,arrivalStop)
            match arrivals with
            | Some a -> a
            | None -> Seq.empty<Trip>

        let getRouteInfo (routeId:string) : BusRoute option =
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

        let getRouteInfoWithCache (routeId:string) : BusRoute option =
            SharedCode.getFromCache routeCache (60.0*24.0) getRouteInfo routeId

        let getStopInfo (stopId:string) : BusStop option =
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

        let getStopInfoWithCache (stopId:string) : BusStop option =
            SharedCode.getFromCache stopCache (60.0*24.0) getStopInfo stopId

        let busParametersToTrips (bus:BusParameters) (minDepartureTime: System.DateTimeOffset)=
            let departureStop = getStopInfoWithCache bus.DepartureStopId
            let arrivalStop = getStopInfoWithCache bus.ArrivalStopId
            let routes = Seq.choose getRouteInfoWithCache bus.RouteIds
            match (departureStop,arrivalStop) with
                | (Some d, Some a) -> getTripsWithCache d routes a
                | _ -> Seq.empty<Trip>
            |> Seq.filter (fun t -> t.Departure >= minDepartureTime)

    module BingMaps =
        let apiKey = SharedCode.getKeyFromProject "BingMaps"
        type TimeSet = {TravelTime: System.TimeSpan; TravelTimeTraffic: System.TimeSpan}
    
        let travelTimeCache = SharedCode.makeNewCache<CarParameters,TimeSet>()
    
        let getCommuteTimes (odPair: CarParameters) =
            let (origin,destination) = (odPair.Origin,odPair.Destination)
            try
                let json =
                    @"http://dev.virtualearth.net/REST/v1/Routes?wayPoint.1="+(System.Web.HttpUtility.HtmlEncode origin)+"&wayPoint.2="+(System.Web.HttpUtility.HtmlEncode destination)+"&key="+apiKey
                    |> ((new System.Net.WebClient()).DownloadString)
                    |> Linq.JObject.Parse
                let travelTime = json.["resourceSets"].[0].["resources"].[0].["travelDuration"] 
                                    |> float
                                    |> System.TimeSpan.FromSeconds
                let travelTimeTraffic = json.["resourceSets"].[0].["resources"].[0].["travelDurationTraffic"]
                                        |> float
                                        |> System.TimeSpan.FromSeconds
                Some {TravelTime=travelTime;TravelTimeTraffic=travelTimeTraffic}
            with |_ -> None


        let getCommuteWithCache (odPair:CarParameters) =
            SharedCode.getFromCache travelTimeCache (60.0) getCommuteTimes odPair

        let carParametersToTrips (cp:CarParameters) (minDepartureTime: System.DateTimeOffset) =
            match getCommuteWithCache cp with
            | Some tt ->
                Seq.singleton {Mode= Seq.singleton Car; Duration = tt.TravelTimeTraffic; Departure = minDepartureTime}
            | None -> Seq.empty
                

    let rec commuteParametersToTrips (c:CommuteParameters) (minDepartureTime: System.DateTimeOffset)=
        match c with
        | CommuteParameters.Bus b -> OneBusAway.busParametersToTrips b minDepartureTime
        | CommuteParameters.Car c -> BingMaps.carParametersToTrips c minDepartureTime
        | CommuteParameters.Walk w -> walkParametersToTrips w minDepartureTime
        | Transfer cp ->
            let subTrips = 
                cp 
                |> Seq.map (fun c -> async{ return commuteParametersToTrips c minDepartureTime})
                |> Async.Parallel
                |> Async.RunSynchronously
                |> Seq.ofArray
            let startTrips =
                Seq.singleton
                    {Departure = minDepartureTime;
                     Duration = new System.TimeSpan(0L);
                     Mode= Seq.empty}
            Seq.fold 
                (fun (currentTrips: Trip seq) (nextTrips: Trip seq) ->
                    currentTrips
                    |> Seq.choose (fun trip -> 
                                        
                                        let possibleNextTrips =  Seq.filter (fun nt -> nt.Departure > trip.Departure + trip.Duration) nextTrips
                                        if Seq.length possibleNextTrips > 0
        | Option ts ->
            ts
            |> Seq.map commuteToTrip
            |> Seq.minBy (fun t -> t.Departure + t.Duration)