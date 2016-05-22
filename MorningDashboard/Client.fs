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
    let commuteBlock() =        
        let updateCommuteBlock (block:Element) (result: string*((string*string) list)) =
            let (routeTitle,arrivalStrings) = result
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
        let getCommuteData = Server.getOneBusAwayBlockData
        refreshBlock 5 getCommuteData updateCommuteBlock