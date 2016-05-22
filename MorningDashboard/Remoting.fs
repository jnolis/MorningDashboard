namespace MorningDashboard

open WebSharper
open WebSharper.Sitelets

module Server =
    let logCall (name:string) =
        System.Diagnostics.Debug.Write ("Server recieved " + name + " call at " + System.DateTime.Now.ToString() + "\n")
    module OneBusAway =
        type ResponseArrivals = {Time: string; TimeUntil: string}
        type Response = {RouteTitle: string; Arrivals: ResponseArrivals list}

        [<Rpc>]
        let getBlockData() =
            async {
                logCall "OneBusAway"
                let route = OneBusAway.getRouteInfo "40_100236"
                let stop = OneBusAway.getStopInfo "1_71335"
                let arrivals = match (route,stop) with
                                    | (Some r, Some s) -> OneBusAway.getArrivalsForStopAndRoute s.Id r.Id
                                    | _ -> Seq.empty<OneBusAway.Arrival>
                                |> Seq.toList
                let result =
                    match (route,stop,arrivals) with
                        | (Some r, Some s, a) -> 
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
                                    {Time = timeString;TimeUntil = timeUntilArrivalString}
                                List.map arrivalToString a
                            Some {RouteTitle = routeTitle; Arrivals = arrivalStrings}
                        | _ -> None
                return result
            }
    module Wunderground =
        type Forecast = {Time: string; Temperature: string; WeatherIcon: string}
        type Current = {Temperature: string; WeatherIcon: string}
        type Response = {Current: Current; Forecast: Forecast list}
        [<Rpc>]
        let getBlockData() =
            async {
                logCall "Wunderground"
                let state = "WA"
                let city = "Seattle"
                let maxHours = 24
                let result =
                    match (Wunderground.getCurrentWeather state city, Wunderground.getHourlyForecast maxHours state city) with
                    | (Some current, Some forecasts) ->
                        let forecastData = 
                            forecasts
                            |> Seq.toList
                            |> List.map (fun forecast -> {Time =forecast.Time.ToString("HH:mm"); Temperature = forecast.Temperature.ToString(); WeatherIcon = forecast.WeatherIcon})
                        let currentData =
                            {Temperature = current.Temperature.ToString(); WeatherIcon = current.WeatherIcon}
                        Some {Current = currentData; Forecast = forecastData}
                    | _ -> None
                return result
            }