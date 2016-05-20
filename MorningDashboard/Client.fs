namespace MorningDashboard

open WebSharper
open WebSharper.JavaScript
open WebSharper.Html.Client


[<JavaScript>]
module Client =
    let commuteBlock() = 
        let output = Div []  -< [Attr.Id "CommuteOutput"]       
        let updateCommuteBlock (block:Element) (routeTitle:string) (arrivalStrings: (string*string) list) =
            let arrivalElements =
                arrivalStrings
                |> List.map (fun arrival ->
                                TR [
                                    TD [Text (fst arrival)]
                                    TD [Text (snd arrival)]
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
        let repeater (commuteBlock:Element) = 
            async {
                let! result = Server.getOneBusAwayBlockData()
                match result with
                | Some (routeTitle,arrivalStrings) -> 
                    updateCommuteBlock commuteBlock routeTitle arrivalStrings
                    ()
                | _ -> ()
                }
            |> Async.Start

        do repeater output
        Div [
            output
                |>! OnBeforeRender (fun dummy -> JS.SetInterval (fun () -> (repeater output)) (15*1000) |> ignore)
            ]