namespace MorningDashboard

open WebSharper
open WebSharper.JavaScript
open WebSharper.Html.Client


[<JavaScript>]
module Client =
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
        let updateCommuteBlock (block:Element) (result: Server.OneBusAway.Response) =
            let (routeTitle,arrivalStrings) = (result.RouteTitle,result.Arrivals)
            let arrivalElements =
                arrivalStrings
                |> List.map (fun arrival ->
                                TR [
                                    TD [Text (arrival.Time)]
                                    TD [Text (arrival.TimeUntil)]
                                ]
                            )
            block.Clear()
            block.Append 
                (Div [
                    Div [
                        Div [
                                H4 [Text routeTitle ]
                            ] -< [Attr.Class "panel-heading"]
                        Table
                            (List.append 
                                    (List.singleton (THead [ TR [ TH [Text "ETA"]; TH [Text "Minutes"]]]))
                                    arrivalElements)
                            -< [Attr.Class "table"]
                        ] -< [Attr.Class "panel panel-default"]
                    ] -< [Attr.Class "col-md-6"])
        let getCommuteData = Server.OneBusAway.getBlockData
        refreshBlock 15 getCommuteData updateCommuteBlock
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
            let header = 
                [
                H1 [I [Attr.Class ("wi " + result.Current.WeatherIcon)]]
                H4 [Text (result.Current.Temperature + "°")]
                ]
            block.Clear()
            block.Append 
                (Div [
                    Div [
                        Div header -< [Attr.Class "panel-heading"]
                        Table
                            (List.append 
                                    (List.singleton (THead [ TR [ TH [Text "Hour"]; TH [Text "Weather"]; TH [Text "Temperature"]]]))
                                    forecastElements)
                            -< [Attr.Class "table"]
                        ] -< [Attr.Class "panel panel-default"]
                    ] -< [Attr.Class "col-md-6"])
        let getData = Server.Wunderground.getBlockData
        refreshBlock (60*15) getData updateBlock