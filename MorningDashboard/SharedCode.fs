namespace MorningDashboard

open Newtonsoft.Json

module SharedCode =
    let adjustToSystemTime (currentReferenceTime: System.DateTimeOffset) (adjustedTime: System.DateTimeOffset) =
        let now = System.DateTimeOffset.Now
        let adjustmentSpan = currentReferenceTime - now
        adjustedTime - adjustmentSpan
    let getUpLocation (levels:int) = 
        let upString =  Seq.replicate levels @"..\"
                        |> Seq.fold (+) @"\"
        let currentLocation = try Some (System.IO.Path.GetFullPath((new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath)) with
                                | :? System.NotSupportedException -> None
        match currentLocation with
                    | Some c ->   c
                                |> System.IO.Path.GetFullPath
                                |> (fun x-> x + upString)
                                |> System.IO.Path.GetFullPath
                    | None -> ""

    let solutionLocation () = getUpLocation 4
    let projectLocation () = getUpLocation 3

    let getKey (keyFile:string) (keyName:string) =
        keyFile
        |> System.IO.File.ReadAllText
        |> Linq.JObject.Parse
        |> (fun x -> x.["Keys"])
        |> Seq.filter (fun key -> (string key.["Name"]) = keyName)
        |> Seq.head
        |> (fun key -> string key.["Key"])

    let getKeyFile () =
        System.IO.Path.GetFullPath (projectLocation() + @"\Keys.json")

    let getKeyFromProject (keyName:string) =
        getKey (getKeyFile ()) keyName