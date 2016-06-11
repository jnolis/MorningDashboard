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
                        ClientSide <@ Client.oneBusAwayBlock() @>
                        ClientSide <@ Client.wundergroundBlock() @>
                        ClientSide <@ Client.currentTimeBlock() @>
                        ClientSide <@ Client.calendarBlock() @>
                        ClientSide <@ Client.twitterBlock() @>
                        ClientSide <@ Client.trafficMapBlock() @>
                        ] -< [Attr.Id "bodyTemplate"]
        Content.WithTemplate HomePageTemplate body

    [<Website>]
    let Main =
        Sitelet.Infer (fun (context:Context<EndPoint>) (endpoint:EndPoint) -> HomePage)