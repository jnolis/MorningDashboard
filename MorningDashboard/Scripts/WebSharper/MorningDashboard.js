(function()
{
 var Global=this,Runtime=this.IntelliFactory.Runtime,Html,Client,Operators,List,T,Tags,Attr,Concurrency,Remoting,AjaxRemotingProvider,setInterval;
 Runtime.Define(Global,{
  MorningDashboard:{
   Client:{
    commuteBlock:function()
    {
     var output,arg10,repeater;
     arg10=Runtime.New(T,{
      $:0
     });
     output=Operators.add(Tags.Tags().NewTag("div",arg10),List.ofArray([Attr.Attr().NewAttr("id","CommuteOutput")]));
     repeater=function()
     {
      return Concurrency.Start(Concurrency.Delay(function()
      {
       return Concurrency.Bind(AjaxRemotingProvider.Async("MorningDashboard:0",[]),function(_arg1)
       {
        var routeTitle,arrivalStrings,arrivalElements,arg104,arg105,arg106,arg107,arg108,arg109,arg10a,arg10b,arg10c;
        if(_arg1.$==1)
         {
          routeTitle=_arg1.$0[0];
          arrivalStrings=_arg1.$0[1];
          arrivalElements=List.map(function(arrival)
          {
           var arg101,arg102,x,arg103,x1;
           x=arrival[0];
           arg102=List.ofArray([Tags.Tags().text(x)]);
           x1=arrival[1];
           arg103=List.ofArray([Tags.Tags().text(x1)]);
           arg101=List.ofArray([Tags.Tags().NewTag("td",arg102),Tags.Tags().NewTag("td",arg103)]);
           return Tags.Tags().NewTag("tr",arg101);
          },arrivalStrings);
          output["HtmlProvider@33"].Clear(output.get_Body());
          arg107=List.ofArray([Tags.Tags().text(routeTitle)]);
          arg106=List.ofArray([Tags.Tags().NewTag("h4",arg107)]);
          arg10b=List.ofArray([Tags.Tags().text("ETA")]);
          arg10c=List.ofArray([Tags.Tags().text("Minutes")]);
          arg10a=List.ofArray([Tags.Tags().NewTag("th",arg10b),Tags.Tags().NewTag("th",arg10c)]);
          arg109=List.ofArray([Tags.Tags().NewTag("tr",arg10a)]);
          arg108=List.append(List.singleton(Tags.Tags().NewTag("thead",arg109)),arrivalElements);
          arg105=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg106),List.ofArray([Attr.Attr().NewAttr("class","panel-heading")])),Operators.add(Tags.Tags().NewTag("table",arg108),List.ofArray([Attr.Attr().NewAttr("class","table")]))]);
          arg104=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg105),List.ofArray([Attr.Attr().NewAttr("class","panel panel-default")]))]);
          output.AppendI(Operators.add(Tags.Tags().NewTag("div",arg104),List.ofArray([Attr.Attr().NewAttr("class","col-md-6")])));
          return Concurrency.Return(null);
         }
        else
         {
          return Concurrency.Return(null);
         }
       });
      }),{
       $:0
      });
     };
     repeater(null);
     Operators.OnAfterRender(function()
     {
      setInterval(repeater,3*1000*1000);
     },output);
     return output;
    }
   }
  }
 });
 Runtime.OnInit(function()
 {
  Html=Runtime.Safe(Global.WebSharper.Html);
  Client=Runtime.Safe(Html.Client);
  Operators=Runtime.Safe(Client.Operators);
  List=Runtime.Safe(Global.WebSharper.List);
  T=Runtime.Safe(List.T);
  Tags=Runtime.Safe(Client.Tags);
  Attr=Runtime.Safe(Client.Attr);
  Concurrency=Runtime.Safe(Global.WebSharper.Concurrency);
  Remoting=Runtime.Safe(Global.WebSharper.Remoting);
  AjaxRemotingProvider=Runtime.Safe(Remoting.AjaxRemotingProvider);
  return setInterval=Runtime.Safe(Global.setInterval);
 });
 Runtime.OnLoad(function()
 {
  return;
 });
}());
