namespace MorningDashboard

open Newtonsoft.Json

module SharedCode =
    let seqTopN (n:int) (s: 'T seq) =
        if Seq.length s > n then Seq.take n s
        else s

    let makeNewCache<'Key,'Data when 'Key: equality > () =
        new System.Collections.Generic.Dictionary<'Key,System.DateTimeOffset*'Data>()
    let getFromCache  (cache: System.Collections.Generic.Dictionary<'I,System.DateTimeOffset*'O>) (secondsUntilOutdated: float) (map: 'I -> 'O option) (input: 'I) =
        if cache.ContainsKey input then
            let (cacheTime,cacheValue) = cache.Item input
            if cacheTime.AddSeconds(secondsUntilOutdated) <= System.DateTimeOffset.Now then
                let newValue = map input
                do cache.Remove input |> ignore
                if newValue.IsSome then do cache.Add(input,(System.DateTimeOffset.Now,newValue.Value))
                newValue
            else Some cacheValue
        else
            let newValue = map input
            if newValue.IsSome then do cache.Add(input,(System.DateTimeOffset.Now,newValue.Value))
            newValue

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

    let projectLocation () = getUpLocation 2

    let getKey (keyFile:string) (keyName:string) =
        keyFile
        |> System.IO.File.ReadAllText
        |> Linq.JObject.Parse
        |> (fun x -> x.["Keys"])
        |> Seq.filter (fun key -> (string key.["Name"]) = keyName)
        |> Seq.head
        |> (fun key -> string key.["Key"])

    let getKeyFile () =
        try
            System.Web.HttpContext.Current.Request.PhysicalApplicationPath + @"Content/Keys.json"
        with
           | _ ->  (projectLocation()) + "Content\Keys.json"
    let getKeyFromProject (keyName:string) =
        getKey (getKeyFile ()) keyName

    let formatter (s:string seq) =
        match Seq.length s with
        | 0 -> ""
        | 1 -> Seq.head s
        | 2 -> (Seq.head s) + " and " + (Seq.last s)
        | _ ->
            let last = Seq.last s
            let secondToLast = Seq.item (Seq.length s - 2) s
            let first = Seq.head s
            let rest = s |> Seq.mapi (fun id str -> (id,str)) |> Seq.filter (fun (id,str) -> id <> 0 && id < (Seq.length s - 2)) |> Seq.map snd
            Seq.fold (fun current next -> current + ", " + next) first (Seq.append rest (Seq.singleton (secondToLast + ", and " + last)))