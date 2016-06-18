namespace MorningDashboard

open WebSharper
open WebSharper.Sitelets

type EndPoint =
    | Index
    | Dashboard of code: string


module Site =
    open WebSharper.Html.Server

    let HomePage =
        let homePageTemplate =
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
        Content.WithTemplate homePageTemplate body



    let ErrorPage = 
        let errorPageTemplate =
              Content.Template<Element>("~/Main.html").With("body", id)
        let body = Div [
                        H5 [Text "Invalid code"]
                        ] -< [Attr.Id "bodyTemplate"; Attr.Class "container"]
        Content.WithTemplate errorPageTemplate body

    [<Website>]
    let Main =
        Sitelet.Infer (fun (context:Context<EndPoint>) (endpoint:EndPoint) -> 
                            match endpoint with 
                                | Index -> ErrorPage
                                | Dashboard code -> 
                                    if code = Server.config.UrlCode then HomePage else ErrorPage)