namespace MorningDashboard

open Newtonsoft.Json

module Wunderground =
    type Location = {City:string; State: string}
    let apiKey = SharedCode.getKeyFromProject "Wunderground"
    let wuIconToWeatherIcon (wuString:string) (isDay: bool) =
        match wuString with
            | "chanceflurries" -> ("snow-wind", "night-snow-wind")
            | "chancerain" -> ("rain", "night-alt-rain")
            | "chancesleet" -> ("sleet", "night-alt-sleet")
            | "chancesnow" -> ("snow", "night-alt-snow")
            | "chancetstorms" -> ("thunderstorm", "night-alt-thunderstorm")
            | "clear" -> ("day-sunny", "night-clear")
            | "cloudy" -> ("day-cloudy", "night-alt-cloudy")
            | "flurries" -> ("snow-wind", "night-alt-snow-wind")
            | "fog" -> ("day-fog", "night-fog")
            | "hazy" -> ("day-haze", "day-haze")
            | "mostlycloudy" -> ("day-cloudy", "night-alt-cloudy")
            | "mostlysunny" -> ("day-sunny", "night-clear")
            | "partlycloudy" -> ("day-cloudy", "night-alt-cloudy")
            | "partlysunny" -> ("day-sunny", "night-clear")
            | "sleet" -> ("showers", "night-alt-showers")
            | "rain" -> ("sleet", "night-alt-sleet")
            | "snow" -> ("snow", "night-alt-snow")
            | "sunny" -> ("day-sunny", "night-clear")
            | "tstorms" -> ("thunderstorm", "night-alt-thunderstorm")
            | _ -> ("na", "na")
        |> (if isDay then fst else snd)
        |> (fun x -> "wi-" + x)

    let iconUrlToIsDay (url:string) = 
        let iconFilename = url.Split('/')
                            |> Array.last
        not (iconFilename.StartsWith("nt_"))
    type HourlyForecast = {Time: System.DateTimeOffset; Temperature: int; WeatherIcon: string}
    type DailyForecast = {Time: System.DateTimeOffset; High: int; Low: int; WeatherIcon: string}
    type Current = {Temperature: int; WeatherIcon: string}
    
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
