namespace MorningDashboard

open WebSharper
open WebSharper.Sitelets

module Server =

    [<Rpc>]
    let getOneBusAwayBlockData() =
        async {
            System.Diagnostics.Debug.Write "Server Recieved Call\n"
            let route = OneBusAway.getRouteInfo "40_100236"
            let stop = OneBusAway.getStopInfo "1_71335"
            let arrivals = match (route,stop) with
                                | (Some r, Some s) -> OneBusAway.getArrivalsForStopAndRoute s.Id r.Id
                                | _ -> Seq.empty<OneBusAway.Arrival>
                            |> Seq.toList
            let result =
                match (route,stop,arrivals) with
                    | (Some r, Some s, a) when Seq.length a > 0 -> 
                        let routeTitle = s.Name + " [" + s.Direction + "] " + r.ShortName + ": " + r.LongName
                        let arrivalStrings =
                            let arrivalToString (arrival:OneBusAway.Arrival) =
                                let (showTime, isPredicted) = match arrival.Predicted with
                                                                | Some p -> (p,true)
                                                                | None -> (arrival.Scheduled,false)
                                let timeUntilArrivalString = (showTime - arrival.Current).Minutes.ToString()
                                let timeString = 
                                    let raw = showTime.ToString("HH:mm") 
                                    if isPredicted then raw
                                    else raw + "*"
                                (timeString,timeUntilArrivalString)
                            List.map arrivalToString a
                        Some (routeTitle,arrivalStrings)
                    | _ -> None
            return result
        }