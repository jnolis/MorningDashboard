namespace MorningDashboard

open WebSharper
open WebSharper.Sitelets

type EndPoint =
    | [<EndPoint "/">] Home


module Site =
    open WebSharper.Html.Server

    let HomePage =
        let HomePageTemplate =
              Content.Template<Element>("~/Main.html").With("body", id)


        let body = Div [
                        Div [Attr.Class "col-md-3"] -<
                                [   ClientSide <@ Client.trafficMapBlock() @>
                                    ClientSide <@ Client.oneBusAwayBlock() @>]
                        Div [Attr.Class "col-md-4"] -<
                                [   ClientSide <@ Client.currentTimeBlock() @>
                                    ClientSide <@ Client.wundergroundBlock() @>]
                        Div [Attr.Class "col-md-5"] -<
                                [   ClientSide <@ Client.calendarBlock() @>
                                    ClientSide <@ Client.twitterBlock() @>]
                        
                        
                        
                        ] -< [Attr.Id "bodyTemplate"; Attr.Class "container"]
        Content.WithTemplate HomePageTemplate body

    [<Website>]
    let Main =
        Sitelet.Infer (fun (context:Context<EndPoint>) (endpoint:EndPoint) -> HomePage)