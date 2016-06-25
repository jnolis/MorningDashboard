namespace MorningDashboard

open Newtonsoft.Json

module BingMaps =
    let apiKey = SharedCode.getKeyFromProject "BingMaps"

    type OdPair = {Origin: string; Destination: string; Name: string}
    type TimeSet = {TravelTime: int; TravelTimeTraffic: int}
    
    let travelTimeCache = SharedCode.makeNewCache<OdPair,TimeSet>()

    let getCommuteTimes (odPair: OdPair) =
        let (origin,destination) = (odPair.Origin,odPair.Destination)
        try
            let json =
                @"http://dev.virtualearth.net/REST/v1/Routes?wayPoint.1="+(System.Web.HttpUtility.HtmlEncode origin)+"&wayPoint.2="+(System.Web.HttpUtility.HtmlEncode destination)+"&key="+apiKey
                |> ((new System.Net.WebClient()).DownloadString)
                |> Linq.JObject.Parse
            let travelTime = json.["resourceSets"].[0].["resources"].[0].["travelDuration"] |> int
            let travelTimeTraffic = json.["resourceSets"].[0].["resources"].[0].["travelDurationTraffic"] |> int
            Some {TravelTime=travelTime;TravelTimeTraffic=travelTimeTraffic}
        with |_ -> None


    let getCommuteWithCache (odPair:OdPair) =
        SharedCode.getFromCache travelTimeCache (60.0) getCommuteTimes odPair
