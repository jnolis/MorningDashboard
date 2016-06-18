namespace MorningDashboard

open Newtonsoft.Json

module OneBusAway =
    let apiKey = SharedCode.getKeyFromProject "OneBusAway"

    
    type Commute = {Name: string; StopId: string; RouteIds: string seq}
    type Arrival = {Name: string; Current: System.DateTimeOffset; Scheduled: System.DateTimeOffset; Predicted: System.DateTimeOffset option}
    type Route = {Id: string; LongName: string; ShortName: string; Description: string}
    type Stop = {Id: string; Name: string; Direction: string}
    
    let routeCache =SharedCode.makeNewCache<string,Route>()
    let stopCache = SharedCode.makeNewCache<string,Stop>()
    let arrivalCache = SharedCode.makeNewCache<Stop*(Route seq),Arrival seq>()

    let getArrivalsForStopAndRoutes (stop: Stop) (routes:Route seq) =
        let json =
            @"http://api.pugetsound.onebusaway.org/api/where/arrivals-and-departures-for-stop/"+stop.Id+".json?key="+apiKey
            |> ((new System.Net.WebClient()).DownloadString)
            |> Linq.JObject.Parse
        if string json.["code"] <> "200" then Seq.empty<Arrival>
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
                                if scheduledDepartureInt = 0L then None else
                                        Some {Name= route.ShortName; Current = currentTime; Scheduled= scheduledDeparture; Predicted = predictedDeparture}
                            | None -> None
                                    )
        
    let getArrivalsForStopAndRoutesWithCache (stop: Stop) (routes:Route seq) =
        let arrivals = SharedCode.getFromCache arrivalCache (14.5) (fun (s,r) -> getArrivalsForStopAndRoutes s r |> Some) (stop, routes)
        match arrivals with
        | Some a -> a
        | None -> Seq.empty<Arrival>

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