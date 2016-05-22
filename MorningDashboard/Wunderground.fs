namespace MorningDashboard

open Newtonsoft.Json

module Wunderground =
    let apiKey = SharedCode.getKeyFromProject "Wunderground"
    let wuIconToWeatherIcon (wuString:string) (isDay: bool) =
        match wuString with
            | "chanceflurries" -> ("snow-wind", "snow-wind")
            | "chancerain" -> ("rain", "rain")
            | "chancesleet" -> ("sleet", "sleet")
            | "chancesnow" -> ("snow", "snow")
            | "chancetstorms" -> ("thunderstorm", "thunderstorm")
            | "clear" -> ("day-sunny", "night-clear")
            | "cloudy" -> ("day-cloudy", "night-alt-cloudy")
            | "flurries" -> ("snow-wind", "snow-wind")
            | "fog" -> ("day-fog", "night-fog")
            | "hazy" -> ("day-haze", "day-haze")
            | "mostlycloudy" -> ("day-cloudy", "night-alt-cloudy")
            | "mostlysunny" -> ("day-sunny", "night-clear")
            | "partlycloudy" -> ("day-cloudy", "night-alt-cloudy")
            | "partlysunny" -> ("day-sunny", "night-clear")
            | "sleet" -> ("showers", "showers")
            | "rain" -> ("sleet", "sleet")
            | "snow" -> ("snow", "snow")
            | "sunny" -> ("day-sunny", "night-clear")
            | "tstorms" -> ("thunderstorm", "thunderstorm")
            | _ -> ("na", "na")
        |> (if isDay then fst else snd)
        |> (fun x -> "wi-" + x)

    let iconUrlToIsDay (url:string) = 
        let iconFilename = url.Split('/')
                            |> Array.last
        not (iconFilename.StartsWith("nt_"))
    type Forecast = {Time: System.DateTimeOffset; Temperature: int; WeatherIcon: string}
    type Current = {Temperature: int; WeatherIcon: string}
    let getCurrentWeather (state:string) (city:string) : Current option =
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
    let getHourlyForecast (numHours: int) (state:string) (city:string) : (Forecast seq) option =
        try
                @"http://api.wunderground.com/api/"+apiKey+"/hourly/q/"+state+"/"+ city+".json"
                |> ((new System.Net.WebClient()).DownloadString)
                |> Linq.JObject.Parse
                |> (fun data -> data.["hourly_forecast"])
                |> Seq.map (fun hourData ->
                    let time = hourData.["FCTTIME"].["epoch"]
                                |> int64
                                |> System.DateTimeOffset.FromUnixTimeSeconds
                                |> (fun x -> System.TimeZoneInfo.ConvertTime(x,SharedCode.timeZone))
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
                |> (fun s -> if Seq.length s > numHours then Seq.take numHours s else s)
                |> Some
        with
        | _ -> None