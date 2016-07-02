namespace MorningDashboard

open WebSharper
open WebSharper.Sitelets

type Key = {Name: string; Key: string}

type Commute =
    {
    Name: string;
    Bus: OneBusAway.CommuteId;
    Car: BingMaps.OdPair;
    }
type Config =
    {
        Keys: Key seq;
        TwitterConfig: Twitter.TwitterConfig;
        Calendars: Calendar.CalendarInfo seq;
        Commutes: Commute seq;
        WeatherLocation: Wunderground.Location;
        TimeFormat: string;
        TimeFormatSmall: string;
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
        type TripMethod = | Bus of string | Car
        type Departure =
            | Now
            | Scheduled of System.DateTimeOffset
            | Predicted of System.DateTimeOffset
        type Trip = {Departure: Departure; Duration: System.TimeSpan option; Method: TripMethod}
        type TravelResponse = {Method: TripMethod; Departure: string; Arrival: string; Accent: bool}
        type Response =
            {
            RouteTitle: string
            TravelResponses: TravelResponse list
            }
        let tripToTravelResponse (now: System.DateTimeOffset) (trip:Trip) =
            let timeToString (asterisk: bool) (t:System.DateTimeOffset)  = 
                t.ToString(config.TimeFormatSmall) + (if asterisk then "*" else "") + " (" + (t-now).Minutes.ToString() + ")"
            {Method = trip.Method;
             Departure = match trip.Departure with
                            | Now -> "Now"
                            | Predicted t -> timeToString false t
                            | Scheduled t -> timeToString true t;
             Arrival =
                match (trip.Departure,trip.Duration) with
                        | (_, None) -> "-"
                        | (Now, Some d) -> timeToString false (now + d)
                        | (Predicted t, Some d) -> timeToString false (t + d)
                        | (Scheduled t, Some d) -> timeToString true (t + d);
             Accent = match trip.Departure with
                        | Now -> false
                        | Predicted t -> 
                            let timeUntil = (t-now)
                            timeUntil.Minutes <= 5 && timeUntil.Minutes >= 0
                        | Scheduled t -> 
                            let timeUntil = (t-now)
                            timeUntil.Minutes <= 5 && timeUntil.Minutes >= 0
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
                                            let duration = tt.TravelTimeTraffic
                                            {
                                                Method = Car;
                                                Departure = Now;
                                                Duration = Some duration;
                                                }
                                            |> List.singleton
                                    | None -> List.empty<Trip>
                                return result
                                }
                        let oneBusAway (busCommute:OneBusAway.CommuteId) = 
                            async {
                                    let formatResponse (commute:OneBusAway.CommuteId) (r:OneBusAway.Route seq) (s:OneBusAway.Stop) (a: OneBusAway.Commute seq) =
                                        let arrivalToTrip (arrival:OneBusAway.Commute) =
                                            {Method = Bus arrival.Name; 
                                             Departure = match arrival.Type with 
                                                            | OneBusAway.TripType.Predicted -> Predicted arrival.Departure 
                                                            | OneBusAway.TripType.Scheduled -> Scheduled arrival.Departure;
                                             Duration = match arrival.Arrival with
                                                            | Some a -> Some (a - arrival.Departure)
                                                            | None -> None}
                                        (List.ofSeq a)
                                        |> List.map arrivalToTrip
                                    let arrivalStop = OneBusAway.getStopInfoWithCache busCommute.ArrivalStopId
                                    let routes = Seq.choose OneBusAway.getRouteInfoWithCache busCommute.RouteIds
                                    let result = 
                                        match OneBusAway.getStopInfoWithCache busCommute.DepartureStopId with
                                        | Some stop ->
                                            let arrivals = OneBusAway.getCommutesForStopAndRoutesWithCache stop routes arrivalStop
                                            (formatResponse busCommute routes stop arrivals)
                                        | None -> List.empty<Trip>
                                    return result
                            }
                        let calculateCommute commute =
                            async {
                                let trips = 
                                    [bingMaps commute.Car;oneBusAway commute.Bus]
                                    |> Async.Parallel
                                    |> Async.RunSynchronously
                                    |> Array.toList
                                    |> List.concat
                                let now = System.DateTimeOffset.Now
                                return {RouteTitle= commute.Name; TravelResponses = List.map (tripToTravelResponse now) trips}
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