namespace MorningDashboard

open WebSharper
open WebSharper.Sitelets

module Server =
    let timeFormat = "HH:mm"
    let logCall (name:string) =
        System.Diagnostics.Debug.Write ("Server recieved " + name + " call at " + System.DateTime.Now.ToString() + "\n")
    module OneBusAway =
        type ResponseArrivals = {Time: string; TimeUntil: string}
        type Response = {RouteTitle: string; Arrivals: ResponseArrivals list}

        [<Rpc>]
        let getBlockData () =
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
                                        let raw = showTime.ToString(timeFormat) 
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
                let maxHours = 12
                let result =
                    match (Wunderground.getCurrentWeather state city, Wunderground.getHourlyForecast maxHours state city) with
                    | (Some current, Some forecasts) ->
                        let forecastData = 
                            forecasts
                            |> Seq.toList
                            |> List.map (fun forecast -> {Time =forecast.Time.ToString(timeFormat); Temperature = forecast.Temperature.ToString(); WeatherIcon = forecast.WeatherIcon})
                        let currentData =
                            {Temperature = current.Temperature.ToString(); WeatherIcon = current.WeatherIcon}
                        Some {Current = currentData; Forecast = forecastData}
                    | _ -> None
                return result
            }

    module CurrentTime =
        type Response = {Time: string; Month: string; Day: string; Weekday: string}
        [<Rpc>]
        let getBlockData() =
            async {
                let result =
                    match CurrentTime.getCurrentTime() with
                    | (Some currentTime) ->
                        let time = currentTime.ToString(timeFormat)
                        let weekday = currentTime.ToString("dddd")
                        let month = currentTime.ToString("MMMM")
                        let day = currentTime.ToString("%d")
                        Some {Time = time; Month = month; Day = day; Weekday = weekday}
                    | _ -> None
                return result
            }

    module Calendar =
        type Instance = {Calendar: string; Time: string ; Event: string}
        type Response = {Title: string; Instances: Instance list}
        let generateTimeAndDuration (instance:Calendar.Instance)=
            if instance.IsAllDay then "All day"
            else
                let span = instance.EndTime - instance.StartTime
                let duration =
                    if span.TotalDays >= 1.0 then
                        span.TotalDays.ToString("n2") + " days"
                    else if span.TotalHours >= 1.0 then
                        span.TotalHours.ToString("n2") + " hours"
                    else 
                        span.TotalMinutes.ToString("n2") + " minutes"
                instance.StartTime.ToString(timeFormat) + " (" + duration + ")"
        [<Rpc>]
        let getBlockData() =
            async {
                logCall "Calendar"
                let result = 
                    try
                        let startRange = System.DateTimeOffset.Now.Date |> System.DateTimeOffset
                        let endRange = System.DateTimeOffset.Now.Date.AddDays(1.0) |> System.DateTimeOffset
                        let title = "Agenda for " + startRange.Date.ToString("MMMM d")
                        let instances =
                            (Calendar.getAllCalendars (SharedCode.getKeyFile()) startRange endRange).Instances
                            |> Seq.map (fun instance -> 
                                let calendar = instance.CalendarName
                                let event = instance.EventName
                                let timeAndDuration = generateTimeAndDuration instance
                                {Calendar = calendar; Event = event; Time = timeAndDuration})
                            |> Seq.toList
                        Some {Title = title; Instances = instances}
                    with | _ -> None
                    
                return result
            }