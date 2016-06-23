namespace MorningDashboard

open WebSharper
open WebSharper.Sitelets

type EndPoint =
    | Dashboard of code: string


module Site =
    open WebSharper.Html.Server

    let isMobile (ctx:Context<'T>) =
        let httpContext = ctx.Environment.["HttpContext"] :?> System.Web.HttpContextWrapper
        httpContext.Request.Browser.IsMobileDevice

    let HomePage (isMobile:bool) =
        let homePageTemplate =
              Content.Template<Element>("~/Main.html").With("body", id)


        let body = 
            if not isMobile then
                Div [
                        Div [Attr.Class "col-lg-3"] -<
                                [   ClientSide <@ Client.trafficMapBlock() @>
                                    ClientSide <@ Client.oneBusAwayBlock() @>]
                        Div [Attr.Class "col-lg-4"] -<
                                [   ClientSide <@ Client.currentTimeBlock() @>
                                    ClientSide <@ Client.wundergroundBlock() @>]
                        Div [Attr.Class "col-lg-5"] -<
                                [   ClientSide <@ Client.calendarBlock() @>]
                        ] -< [Attr.Id "bodyTemplate"; Attr.Class "container"]
            else 
                 Div [
                        Div [Attr.Class "col-xs-12"] -<
                                [   
                                    ClientSide <@ Client.currentTimeBlock() @>
                                    ClientSide <@ Client.wundergroundBlock() @>
                                    ClientSide <@ Client.calendarBlock() @>
                                    ClientSide <@ Client.trafficMapBlock() @>
                                    ClientSide <@ Client.oneBusAwayBlock() @>
                                    ]                       
                        ] -< [Attr.Id "bodyTemplate"; Attr.Class "container"]
        Content.WithTemplate homePageTemplate body



    let ErrorPage message= 
        let errorPageTemplate =
              Content.Template<Element>("~/Main.html").With("body", id)
        let body = Div [
                        H5 [Text message]
                        ] -< [Attr.Id "bodyTemplate"; Attr.Class "container"]
        Content.WithTemplate errorPageTemplate body

    [<Website>]
    let Main =
        Sitelet.Infer (fun (context:Context<string>) (code:string) -> 
                                    if code = Server.config.UrlCode then HomePage (isMobile context)
                                    else if code = "" then ErrorPage "Missing code"
                                    else ErrorPage "Invalid code")