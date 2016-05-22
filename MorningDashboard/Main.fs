namespace MorningDashboard

open WebSharper
open WebSharper.Sitelets

type EndPoint =
    | [<EndPoint "/">] Home


module Site =
    open WebSharper.Html.Server

    let HomePage =
        let HomePageTemplate =
              Content.Template<list<Element>>("~/Main.html").With("body", id)


        let body = [
                    Div[
                        ClientSide <@ Client.oneBusAwayBlock() @>
                        ] -< [Attr.Class "container"]
                    Div[
                        ClientSide <@ Client.wundergroundBlock() @>
                        ] -< [Attr.Class "container"]
                    ]
        Content.WithTemplate HomePageTemplate body

    [<Website>]
    let Main =
        Sitelet.Infer (fun (context:Context<EndPoint>) (endpoint:EndPoint) -> HomePage)