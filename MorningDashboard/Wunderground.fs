namespace MorningDashboard

open Newtonsoft.Json

module Wunderground =
    type Location = {City:string; State: string}
    type WeatherIcon = {Icon:string; Accent: bool}
    let apiKey = SharedCode.getKeyFromProject "Wunderground"
    let wuIconToWeatherIcon (wuString:string) (isDay: bool) =
        match wuString with
            | "chanceflurries" -> (("snow-wind", "night-snow-wind"),true)
            | "chancerain" -> (("rain", "night-alt-rain"),true)
            | "chancesleet" -> (("sleet", "night-alt-sleet"),true)
            | "chancesnow" -> (("snow", "night-alt-snow"),true)
            | "chancetstorms" -> (("thunderstorm", "night-alt-thunderstorm"),true)
            | "clear" -> (("day-sunny", "night-clear"),false)
            | "cloudy" -> (("day-cloudy", "night-alt-cloudy"),false)
            | "flurries" -> (("snow-wind", "night-alt-snow-wind"),true)
            | "fog" -> (("day-fog", "night-fog"),false)
            | "hazy" -> (("day-haze", "day-haze"),false)
            | "mostlycloudy" -> (("day-cloudy", "night-alt-cloudy"),false)
            | "mostlysunny" -> (("day-sunny", "night-clear"),false)
            | "partlycloudy" -> (("day-cloudy", "night-alt-cloudy"),false)
            | "partlysunny" -> (("day-sunny", "night-clear"),false)
            | "sleet" -> (("showers", "night-alt-showers"),true)
            | "rain" -> (("sleet", "night-alt-sleet"),true)
            | "snow" -> (("snow", "night-alt-snow"),true)
            | "sunny" -> (("day-sunny", "night-clear"),false)
            | "tstorms" -> (("thunderstorm", "night-alt-thunderstorm"),true)
            | _ -> (("na", "na"),false)
        |> (fun (icon,accent) -> ((if isDay then fst else snd) icon, accent))
        |> (fun (icon,accent) -> {Icon="wi-" + icon;Accent=accent})

    let iconUrlToIsDay (url:string) = 
        let iconFilename = url.Split('/')
                            |> Array.last
        not (iconFilename.StartsWith("nt_"))
    type HourlyForecast = {Time: System.DateTimeOffset; Temperature: int; WeatherIcon: WeatherIcon}
    type DailyForecast = {Time: System.DateTimeOffset; High: int; Low: int; WeatherIcon: WeatherIcon}
    type Current = {Temperature: int; WeatherIcon: WeatherIcon}
    
    let hourlyForecastCache = SharedCode.makeNewCache<Location,HourlyForecast seq>()
    let dailyForecastCache = SharedCode.makeNewCache<Location,DailyForecast seq>()
    let currentCache = SharedCode.makeNewCache<Location,Current>()
    
    let getCurrentWeather (location:Location) : Current option =
        let (city,state) = (location.City,location.State)
        try
            let data =
                @"http://api.wunderground.com/api/"+apiKey+"/conditions/q/"+state+"/"+ city+".json"
                |> ((new System.Net.WebClient()).DownloadString)
                |> Linq.JObject.Parse
                |> (fun data -> data.["current_observation"])
            let temperature =
                data.["temp_f"]
                |> float
                |> round
                |> int
            let icon =
                let weatherString = data.["icon"] |> string
                let isDay = 
                    data.["icon_url"]
                    |> string
                    |> iconUrlToIsDay
                wuIconToWeatherIcon weatherString isDay
            Some {Temperature = temperature; WeatherIcon = icon}
        with | _ -> None

    let getCurrentWeatherWithCache (location:Location) =
        SharedCode.getFromCache currentCache (14.5*60.0) (fun location -> getCurrentWeather location) location

    let getHourlyForecast (location:Location) : (HourlyForecast seq) option =
        let (city,state) = (location.City,location.State)
        try
                @"http://api.wunderground.com/api/"+apiKey+"/hourly/q/"+state+"/"+ city+".json"
                |> ((new System.Net.WebClient()).DownloadString)
                |> Linq.JObject.Parse
                |> (fun data -> data.["hourly_forecast"])
                |> Seq.map (fun hourData ->
                    let time = hourData.["FCTTIME"].["epoch"]
                                |> int64
                                |> System.DateTimeOffset.FromUnixTimeSeconds
                                |> (fun x -> System.TimeZoneInfo.ConvertTime(x,System.TimeZoneInfo.Local))
                    let temperature = hourData.["temp"].["english"] |> int
                    let icon =
                        let weatherString = hourData.["icon"] |> string
                        let isDay = 
                            hourData.["icon_url"]
                            |> string
                            |> iconUrlToIsDay
                        wuIconToWeatherIcon weatherString isDay
                    {Time = time; Temperature = temperature; WeatherIcon = icon}
                    )
                |> Seq.sortBy (fun forecast -> forecast.Time)
                |> Some
        with
        | _ -> None

    let getHourlyForecastWithCache (location:Location) =
        SharedCode.getFromCache hourlyForecastCache (14.5*60.0) (fun l -> getHourlyForecast l) location

    let getDailyForecast (location:Location) : (DailyForecast seq) option =
        let (city,state) = (location.City,location.State)
        try
                @"http://api.wunderground.com/api/"+apiKey+"/forecast/q/"+state+"/"+ city+".json"
                |> ((new System.Net.WebClient()).DownloadString)
                |> Linq.JObject.Parse
                |> (fun data -> data.["forecast"].["simpleforecast"].["forecastday"])
                |> Seq.map (fun dayData ->
                    let time = dayData.["date"].["epoch"]
                                |> int64
                                |> System.DateTimeOffset.FromUnixTimeSeconds
                                |> (fun x -> System.TimeZoneInfo.ConvertTime(x,System.TimeZoneInfo.Local))
                    let high = dayData.["high"].["fahrenheit"] |> int
                    let low = dayData.["low"].["fahrenheit"] |> int
                    let icon =
                        let weatherString = dayData.["icon"] |> string
                        let isDay = 
                            dayData.["icon_url"]
                            |> string
                            |> iconUrlToIsDay
                        wuIconToWeatherIcon weatherString isDay
                    {Time = time; High = high; Low = low; WeatherIcon = icon}
                    )
                |> Seq.sortBy (fun forecast -> forecast.Time)
                |> Some
        with
        | _ -> None

    let getDailyForecastWithCache (location:Location) =
        SharedCode.getFromCache dailyForecastCache (14.5*60.0) (fun location -> getDailyForecast location) location
