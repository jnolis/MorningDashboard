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
                let result =
                    try
                        logCall "OneBusAway"
                        let routesStopsAndArrivals =
                            seq [("40_100236","1_13460");("40_100236","1_71335")]
                            |> Seq.map (fun (routeId, stopId) -> 
                                            let route = OneBusAway.getRouteInfoWithCache routeId
                                            let stop = OneBusAway.getStopInfoWithCache stopId
                                            (route,stop))
                            |> Seq.choose (fun rs -> match rs with | (Some r, Some s) -> Some (r,s) | _ -> None)
                            |> Seq.map (fun (route,stop) -> 
                                            let arrivals = OneBusAway.getArrivalsForStopAndRouteWithCache stop.Id route.Id
                                            (route,stop,arrivals))

                        let result =
                            routesStopsAndArrivals
                            |> Seq.map (fun (r, s, a) ->
                                        let routeTitle = s.Name + " [" + s.Direction + "] " + r.ShortName + ": " + r.LongName
                                        let arrivalStrings =
                                            let arrivalToString (arrival:OneBusAway.Arrival) =
                                                let (showTime, isPredicted) = match arrival.Predicted with
                                                                                | Some p -> (p,true)
                                                                                | None -> (arrival.Scheduled,false)
                                                let timeUntilArrivalString = (showTime - arrival.Current).Minutes.ToString() + "m"
                                                let timeString = 
                                                    let raw = showTime.ToString(timeFormat) 
                                                    if isPredicted then raw
                                                    else raw + "*"
                                                {Time = timeString;TimeUntil = timeUntilArrivalString}
                                            List.map arrivalToString (List.ofSeq a)
                                        {RouteTitle = routeTitle; Arrivals = arrivalStrings})
                            |> List.ofSeq
                        Some result
                    with | _ -> None
                return result
            }
    module Wunderground =
        type Forecast = {Time: string; Temperature: string; WeatherIcon: string}
        type Current = {Temperature: string; WeatherIcon: string; Low: string; High: string}
        type Response = {Current: Current; Forecast: Forecast list}
        [<Rpc>]
        let getBlockData() =
            async {
                logCall "Wunderground"
                let state = "WA"
                let city = "Seattle"
                let maxHours = 12
                let result =
                    match (Wunderground.getCurrentWeatherWithCache state city, Wunderground.getHourlyForecastWithCache state city, Wunderground.getDailyForecastWithCache state city) with
                    | (Some current, Some forecasts, Some daily) ->
                        let forecastData = 
                            if Seq.length forecasts > maxHours then Seq.take maxHours forecasts else forecasts
                            |> Seq.toList
                            |> List.map (fun forecast -> {Time =forecast.Time.ToString(timeFormat); Temperature = forecast.Temperature.ToString(); WeatherIcon = forecast.WeatherIcon})
                        let (dailyLow,dailyHigh) =
                            let today = 
                                daily
                                |> Seq.filter (fun day -> day.Time.Date = System.DateTime.Today)
                                |> (fun today -> if Seq.length today > 0 then Seq.head today |> Some else None)
                            match today with
                            | Some t -> (t.Low.ToString(),t.High.ToString())
                            | None -> ("","")
                        let currentData =
                            {Temperature = current.Temperature.ToString(); WeatherIcon = current.WeatherIcon; Low = dailyLow; High = dailyHigh}
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
        type Instance = {Time: string ; Event: string}
        type Calendar = {Name: string; Instances: Instance list}
        type Response = {Calendars: Calendar list}
        let generateTimeAndDuration (instance:Calendar.Instance)=
            if instance.IsAllDay then "All day"
            else
                let span = instance.EndTime - instance.StartTime
                let duration =
                    if span.TotalDays >= 1.0 then
                        span.TotalDays.ToString("#.#") + "d"
                    else if span.TotalHours >= 1.0 then
                        span.TotalHours.ToString("#.#") + "h"
                    else 
                        span.TotalMinutes.ToString("#") + "m"
                instance.StartTime.ToString(timeFormat) + " (" + duration + ")"
        [<Rpc>]
        let getBlockData() =
            async {
                logCall "Calendar"
                let result = 
                    try
                        let startRange = System.DateTimeOffset.Now.Date |> System.DateTimeOffset
                        let endRange = System.DateTimeOffset.Now.Date.AddDays(1.0) |> System.DateTimeOffset
                        let calendars = 
                            Calendar.getAllCalendarsWithCache startRange endRange
                            |> Seq.choose (
                                    Option.map (fun calendar ->
                                    let instances = 
                                            calendar.Instances
                                            |> Seq.map (fun instance -> {Event = instance.Name; Time = generateTimeAndDuration instance})
                                            |> Seq.toList
                                    {Name= calendar.Name; Instances = instances}))
                            |> Seq.toList
                        Some {Calendars = calendars}
                    with | _ -> None
                    
                return result
            }

    module Twitter =
        type SimpleTweet = {Username: string; Text: string}
        type Response = {Title: string; Tweets: SimpleTweet list}
        let screenNames = Seq.singleton "skyetetra"
        let formatter (s:string seq) =
            match Seq.length s with
            | 0 -> ""
            | 1 -> Seq.head s
            | 2 -> (Seq.head s) + " and " + (Seq.last s)
            | _ ->
                let last = Seq.last s
                let secondToLast = Seq.item (Seq.length s - 2) s
                let first = Seq.head s
                let rest = s |> Seq.mapi (fun id str -> (id,str)) |> Seq.filter (fun (id,str) -> id <> 0 && id < (Seq.length s - 2)) |> Seq.map snd
                Seq.fold (fun current next -> current + ", " + next) first (Seq.append rest (Seq.singleton (secondToLast + ", and " + last)))
        let maxTweets = 10
        [<Rpc>]
        let getBlockData() =
            async {
                logCall "Twitter"
                let result =
                    try
                        let resultTweets =
                            screenNames
                            |> Seq.map (fun screenName -> async {return Twitter.getTweetsFromFriendsOfUser screenName})
                            |> Async.Parallel
                            |> Async.RunSynchronously
                            |> Seq.concat
                            |> Seq.distinct
                            |> Seq.sortByDescending (fun tweet -> tweet.CreatedAt)
                            |> Seq.map (fun tweet -> {Username = tweet.CreatedBy.ScreenName; Text = tweet.Text})
                            |> SharedCode.seqTopN maxTweets
                            |> Seq.toList
                        let title = formatter screenNames
                        Some {Title = title; Tweets = resultTweets}
                    with | _ -> None
                return result
            }