namespace MorningDashboard

open WebSharper
open WebSharper.JavaScript
open WebSharper.Html.Client


[<JavaScript>]
module Client =
    let emptyTable (message: string) = Table [TR [TD [Text (message)]]] -< [Attr.Class "table"]

    let refreshBlock (seconds: int) (getDataFunction: unit -> Async<('T option)>) (updateBlockFunction: (Element -> ('T -> unit))) = 
        let output = Div []      
        let repeater (e:Element) = 
            async {
                let! result = getDataFunction()
                match result with
                | Some x -> 
                    updateBlockFunction e x
                | _ -> ()
                }
            |> Async.Start
        do repeater output
        Div [
            output
                |>! OnBeforeRender (fun dummy -> JS.SetInterval (fun () -> (repeater output)) (seconds*1000) |> ignore)
            ]
    let oneBusAwayBlock() =        
        let updateCommuteBlock (block:Element) (resultCommutes: Server.OneBusAway.Response list) =
            let resultBlocks =
                resultCommutes
                |> List.map( fun result ->
                    let (routeTitle,arrivalStrings) = (result.RouteTitle,result.Arrivals)
                    let arrivalElements =
                        arrivalStrings
                        |> List.map (fun arrival ->
                                        TR [
                                            TD [Text (arrival.Time)]
                                            TD [Text (arrival.TimeUntil)]
                                        ]
                                    )
                    [
                        Div [H5 [Text routeTitle]] -< [Attr.Class "panel-body"];
                        (if List.length arrivalStrings > 0 then
                            Table arrivalElements -< [Attr.Class "table"]
                        else emptyTable "No upcoming arrivals")
                    ]
                    
                        )
                |> List.concat

            do block.Clear()
            block.Append 
                (Div [
                    Div (List.append
                            (List.singleton (Div [H4 [Text "Commute" ]] -< [Attr.Class "panel-heading"]))
                            resultBlocks)
                         -< [Attr.Class "panel panel-default"]
                    ] -< [Attr.Class "col-md-3"])
        let getCommuteData = Server.OneBusAway.getBlockData
        refreshBlock 5 getCommuteData updateCommuteBlock
    let wundergroundBlock() =        
        let updateBlock (block:Element) (result: Server.Wunderground.Response) =
            let forecastElements =
                result.Forecast
                |> List.map (fun forecast ->
                                TR [
                                    TD [Text forecast.Time]
                                    TD [I [Attr.Class ("wi " + forecast.WeatherIcon)]]
                                    TD [Text (forecast.Temperature + "°")]
                                ]
                            )
            let body = 
                [
                Div [H1 [I [Attr.Class ("weather wi " + result.Current.WeatherIcon)]] -< [Attr.Class "highlight"]] -< [Attr.Class "col-md-6"]
                Div [   H4 [Text ("Current: " + result.Current.Temperature + "°")]
                        H4 [Text ("High: " + result.Current.High+ "°")]
                        H4 [Text ("Low: " + result.Current.Low+ "°")]] -< [Attr.Class "col-md-6"]
                ]
            block.Clear()
            block.Append 
                (Div [
                    Div [
                        Div [H4 [Text "Weather"]] -< [Attr.Class "panel-heading"]
                        Div body -< [Attr.Class "panel-body"]
                        Table forecastElements
                            -< [Attr.Class "table table-condensed"]
                        ] -< [Attr.Class "panel panel-default"]
                    ] -< [Attr.Class "col-md-4"])
        let getData = Server.Wunderground.getBlockData
        refreshBlock (60*15) getData updateBlock

    let currentTimeBlock() =        
        let updateBlock (block:Element) (result: Server.CurrentTime.Response) =
            block.Clear()
            block.Append 
                (Div [
                    Div [
                        Div [H4 [Text "Time and date"]] -< [Attr.Class "panel-heading"]
                        Div[
                            Div [H1 [Text result.Time] -< [Attr.Class "highlight"]] -< [Attr.Class "col-md-6"]
                            Div [   H4 [Text result.Weekday]
                                    H4 [Text (result.Month + " " + result.Day)]] -< [Attr.Class "col-md-6"]
                                    ]-< [Attr.Class "panel-body"]
                        ] -< [Attr.Class "panel panel-default"]
                    ] -< [Attr.Class "col-md-5"])
        let getData = Server.CurrentTime.getBlockData
        refreshBlock 5 getData updateBlock

    let calendarBlock() =        
        let updateBlock (block:Element) (result: Server.Calendar.Response) =
            let calendarElements =
                result.Calendars
                |> List.map(fun calendar ->
                        let instanceElements = 
                            calendar.Instances
                            |> List.map (fun instance ->
                                            TR [
                                                TD [Text (instance.Event)]
                                                TD [Text (instance.Time)]
                                            ]
                                        )
                        [
                            Div [H5 [Text calendar.Name]] -< [Attr.Class "panel-body"];
                            (if List.length calendar.Instances > 0 then
                                Table instanceElements -< [Attr.Class "table"]
                            else emptyTable "No events today")
                        ]
                    )
                |> List.concat
            block.Clear()
            block.Append 
                (Div [ 
                    Div
                        (List.append
                            (List.singleton(Div [H4 [Text "Daily events" ]] -< [Attr.Class "panel-heading"]))
                            calendarElements)
                         -< [Attr.Class "panel panel-default"]
                    ] -< [Attr.Class "col-md-5"])
        let getData = Server.Calendar.getBlockData
        refreshBlock (15*60) getData updateBlock

    let twitterBlock() =        
        let updateBlock (block:Element) (result: Server.Twitter.Response) =
            let tweetElements =
                    let tweetElements = 
                        result.Tweets
                        |> List.map (fun tweet ->
                                        TR [
                                            TD [Text (tweet.Username + ": " + tweet.Text)]
                                        ]
                                    )
                    [
                        Div [H5 [Text result.Title]] -< [Attr.Class "panel-body"];
                        (if List.length tweetElements > 0 then Table tweetElements -< [Attr.Class "table table-condensed"] else emptyTable "No tweets available")
                    ]

            block.Clear()
            block.Append 
                (Div [ 
                    Div
                        (List.append
                            (List.singleton(Div [H4 [Text "Recent Tweets" ]] -< [Attr.Class "panel-heading"]))
                            tweetElements)
                         -< [Attr.Class "panel panel-default"]
                    ] -< [Attr.Class "col-md-5"])
        let getData = Server.Twitter.getBlockData
        refreshBlock (10) getData updateBlock