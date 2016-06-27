namespace MorningDashboard

open WebSharper
open WebSharper.Sitelets

type Key = {Name: string; Key: string}

type Commute =
    {
    Name: string;
    BusRoute: OneBusAway.Commute;
    CarRoute: BingMaps.OdPair;
    }
type Config =
    {
        Keys: Key seq;
        TwitterConfig: Twitter.TwitterConfig;
        Calendars: Calendar.CalendarInfo seq;
        Commutes: Commute seq;
        WeatherLocation: Wunderground.Location;
        TimeFormat: string;
        UrlCode: string;
    }


module Server =
    
    let config = 
        SharedCode.getKeyFile()
        |> System.IO.File.ReadAllText
        |> (fun s -> Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(s))

    let logCall (name:string) =
        System.Diagnostics.Debug.Write ("Server recieved " + name + " call at " + System.DateTime.Now.ToString() + "\n")
    module Commute =
        type ResponseArrival = {Time: string; TimeUntil: string; Accent: bool; Name:string}
        type CarResponse = {Name: string; Time: string; TrafficTime: string}
        type TravelResponse = 
            | Bus of ResponseArrival
            | Car of CarResponse
        type Response =
            {
            RouteTitle: string
            TravelResponses: TravelResponse list
            }
        [<Rpc>]
        let getBlockData () =
            async {
                let result =
                    try
                        let bingMaps (carCommute)=
                            async {
                                let result =
                                    match BingMaps.getCommuteWithCache carCommute with
                                    | Some tt ->
                                            {
                                                Name = "car";
                                                Time = (System.Math.Ceiling ((float tt.TravelTime)/60.0)).ToString()+"m";
                                                TrafficTime=(System.Math.Ceiling ((float tt.TravelTimeTraffic)/60.0)).ToString()+"m"
                                                }
                                            |> Car
                                            |> List.singleton
                                    | None -> List.empty<TravelResponse>
                                return result
                                }
                        let oneBusAway (busCommute:OneBusAway.Commute) = 
                            async {
                                    let formatResponse (commute:OneBusAway.Commute) (r:OneBusAway.Route seq) (s:OneBusAway.Stop) a =
                                        let routeTitle = commute.Name
                                        let arrivalToString (arrival:OneBusAway.Arrival) =
                                            let (showTime, isPredicted) = match arrival.Predicted with
                                                                            | Some p -> (p,true)
                                                                            | None -> (arrival.Scheduled,false)
                                            let timeUntilArrivalString = (showTime - arrival.Current).Minutes.ToString() + "m"
                                            let timeString = 
                                                let raw = showTime.ToString(config.TimeFormat) 
                                                if isPredicted then raw
                                                else raw + "*"
                                            {Name = arrival.Name; 
                                                Time = timeString;
                                                TimeUntil = timeUntilArrivalString;
                                                Accent = (showTime - arrival.Current).Minutes <= 5}
                                        (List.ofSeq a)
                                        |> List.map arrivalToString
                                        |> List.map Bus

                                    let routes = Seq.choose OneBusAway.getRouteInfoWithCache busCommute.RouteIds
                                    let result = 
                                        match OneBusAway.getStopInfoWithCache busCommute.StopId with
                                        | Some stop ->
                                            let arrivals = OneBusAway.getArrivalsForStopAndRoutesWithCache stop routes
                                            (formatResponse busCommute routes stop arrivals)
                                        | None -> List.empty<TravelResponse>
                                    return result
                            }
                        let calculateCommute commute =
                            async {
                                let travelResponses = 
                                    [bingMaps commute.CarRoute;oneBusAway commute.BusRoute]
                                    |> Async.Parallel
                                    |> Async.RunSynchronously
                                    |> Array.toList
                                    |> List.concat
                                return {RouteTitle= commute.Name; TravelResponses = travelResponses}
                            }
                        config.Commutes
                        |> Seq.map calculateCommute
                        |> Async.Parallel
                        |> Async.RunSynchronously
                        |> List.ofArray
                        |> Some
                    with | _ -> None
                return result
            }
    module Wunderground =
        type Forecast = {Time: string; Temperature: string; WeatherIcon: string; Accent: bool}
        type Current = {Temperature: string; WeatherIcon: string; Accent: bool; Low: string; High: string}
        type Response = {Current: Current; Forecast: Forecast list}
        [<Rpc>]
        let getBlockData() =
            async {
                logCall "Wunderground"
                let maxHours = 12
                let result =
                    match (Wunderground.getCurrentWeatherWithCache config.WeatherLocation, Wunderground.getHourlyForecastWithCache config.WeatherLocation, Wunderground.getDailyForecastWithCache config.WeatherLocation) with
                    | (Some current, Some forecasts, Some daily) ->
                        let forecastData = 
                            if Seq.length forecasts > maxHours then Seq.take maxHours forecasts else forecasts
                            |> Seq.toList
                            |> List.map (fun forecast -> {Time =forecast.Time.ToString(config.TimeFormat); Temperature = forecast.Temperature.ToString(); WeatherIcon = forecast.WeatherIcon.Icon; Accent = forecast.WeatherIcon.Accent})
                        let (dailyLow,dailyHigh) =
                            let today = 
                                daily
                                |> Seq.filter (fun day -> day.Time.Date = System.DateTime.Today)
                                |> (fun today -> if Seq.length today > 0 then Seq.head today |> Some else None)
                            match today with
                            | Some t -> (t.Low.ToString(),t.High.ToString())
                            | None -> ("","")
                        let currentData =
                            {Temperature = current.Temperature.ToString(); WeatherIcon = current.WeatherIcon.Icon; Accent = current.WeatherIcon.Accent; Low = dailyLow; High = dailyHigh}
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
                        let time = currentTime.ToString(config.TimeFormat)
                        let weekday = currentTime.ToString("dddd")
                        let month = currentTime.ToString("MMMM")
                        let day = currentTime.ToString("%d")
                        Some {Time = time; Month = month; Day = day; Weekday = weekday}
                    | _ -> None
                return result
            }

    module Calendar =
        type Instance = {Time: string ; Domain: string; Event: string}
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
                    else if span.TotalMinutes >= 1.0 then
                        span.TotalMinutes.ToString("#") + "m"
                    else "0m"
                instance.StartTime.ToString(config.TimeFormat) + " (" + duration + ")"
        [<Rpc>]
        let getBlockData() =
            async {
                logCall "Calendar"
                let getCalendarFromDateAndName (date: System.DateTime) (name: string) = 
                    try
                        let fullCalendar = Calendar.getCombinedCalendarWithCache date config.Calendars
                        let instances = 
                                fullCalendar.Instances
                                |> Seq.map (fun instance -> {Event = instance.Name; Time = generateTimeAndDuration instance; Domain = instance.Domain})
                                |> Seq.toList
                        Some {Name=name; Instances=instances}
                    with | _ -> None

                let todayCalendarOption = getCalendarFromDateAndName System.DateTimeOffset.Now.Date "Today"
                let tomorrowCalendarOption = getCalendarFromDateAndName (System.DateTimeOffset.Now.AddDays(1.0).Date) "Tomorrow"
                
                let result = 
                    match (todayCalendarOption,tomorrowCalendarOption) with
                    | (Some today, Some tomorrow) -> 
                        if (List.length today.Instances ) + (List.length tomorrow.Instances ) <= 10 then
                            Some { Calendars = [today;tomorrow]}
                        else Some {Calendars = [today]}
                    | (Some today, None) -> Some {Calendars =[today]}
                    | _ -> None
                return result
            }

    module Twitter =
        type SimpleTweet = {Username: string; Text: string}
        type Response = {Title: string; Tweets: SimpleTweet list}
        let maxTweets = 10
        [<Rpc>]
        let getBlockData() =
            async {
                logCall "Twitter"
                let result =
                    try
                        let configs = Seq.singleton config.TwitterConfig
                        let resultTweets =
                            configs
                            |> Seq.map (fun config -> async {return Twitter.getTweetsFromConfig config})
                            |> Async.Parallel
                            |> Async.RunSynchronously
                            |> Seq.concat
                            |> Seq.distinct
                            |> Seq.sortByDescending (fun tweet -> tweet.CreatedAt)
                            |> Seq.map (fun tweet -> {Username = tweet.CreatedBy.ScreenName; Text = tweet.Text})
                            |> SharedCode.seqTopN maxTweets
                            |> Seq.toList
                        let title =
                            Seq.singleton config.TwitterConfig.ScreenName
                            |> Seq.append config.TwitterConfig.JoinedScreenNames
                            |> SharedCode.formatter
                        Some {Title = title; Tweets = resultTweets}
                    with | _ -> None
                return result
            }