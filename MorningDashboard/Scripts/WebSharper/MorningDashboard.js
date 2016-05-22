(function()
{
 var Global=this,Runtime=this.IntelliFactory.Runtime,MorningDashboard,Client,Remoting,AjaxRemotingProvider,List,Html,Client1,Tags,Operators,Attr,T,Concurrency,setInterval;
 Runtime.Define(Global,{
  MorningDashboard:{
   Client:{
    commuteBlock:function()
    {
     return Client.refreshBlock(5,function()
     {
      return AjaxRemotingProvider.Async("MorningDashboard:0",[]);
     },function(block)
     {
      return function(result)
      {
       var routeTitle,arrivalStrings,arrivalElements,arg103,arg104,arg105,arg106,arg107,arg108,arg109,arg10a,arg10b;
       routeTitle=result[0];
       arrivalStrings=result[1];
       arrivalElements=List.map(function(arrival)
       {
        var arg10,arg101,x,arg102,x1;
        x=arrival[0];
        arg101=List.ofArray([Tags.Tags().text(x)]);
        x1=arrival[1];
        arg102=List.ofArray([Tags.Tags().text(x1)]);
        arg10=List.ofArray([Tags.Tags().NewTag("td",arg101),Tags.Tags().NewTag("td",arg102)]);
        return Tags.Tags().NewTag("tr",arg10);
       },arrivalStrings);
       block["HtmlProvider@33"].Clear(block.get_Body());
       arg106=List.ofArray([Tags.Tags().text(routeTitle)]);
       arg105=List.ofArray([Tags.Tags().NewTag("h4",arg106)]);
       arg10a=List.ofArray([Tags.Tags().text("ETA")]);
       arg10b=List.ofArray([Tags.Tags().text("Minutes")]);
       arg109=List.ofArray([Tags.Tags().NewTag("th",arg10a),Tags.Tags().NewTag("th",arg10b)]);
       arg108=List.ofArray([Tags.Tags().NewTag("tr",arg109)]);
       arg107=List.append(List.singleton(Tags.Tags().NewTag("thead",arg108)),arrivalElements);
       arg104=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg105),List.ofArray([Attr.Attr().NewAttr("class","panel-heading")])),Operators.add(Tags.Tags().NewTag("table",arg107),List.ofArray([Attr.Attr().NewAttr("class","table")]))]);
       arg103=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg104),List.ofArray([Attr.Attr().NewAttr("class","panel panel-default")]))]);
       return block.AppendI(Operators.add(Tags.Tags().NewTag("div",arg103),List.ofArray([Attr.Attr().NewAttr("class","col-md-6")])));
      };
     });
    },
    refreshBlock:function(seconds,getDataFunction,updateBlockFunction)
    {
     var x,output,repeater,arg10,f;
     x=Runtime.New(T,{
      $:0
     });
     output=Tags.Tags().NewTag("div",x);
     repeater=function(e)
     {
      var arg00;
      arg00=Concurrency.Delay(function()
      {
       return Concurrency.Bind(getDataFunction(null),function(_arg1)
       {
        var _,x1;
        if(_arg1.$==1)
         {
          x1=_arg1.$0;
          (updateBlockFunction(e))(x1);
          _=Concurrency.Return(null);
         }
        else
         {
          _=Concurrency.Return(null);
         }
        return _;
       });
      });
      return Concurrency.Start(arg00,{
       $:0
      });
     };
     repeater(output);
     f=function()
     {
      var value;
      value=setInterval(function()
      {
       return repeater(output);
      },seconds*1000);
      return;
     };
     Operators.OnBeforeRender(f,output);
     arg10=List.ofArray([output]);
     return Tags.Tags().NewTag("div",arg10);
    }
   }
  }
 });
 Runtime.OnInit(function()
 {
  MorningDashboard=Runtime.Safe(Global.MorningDashboard);
  Client=Runtime.Safe(MorningDashboard.Client);
  Remoting=Runtime.Safe(Global.WebSharper.Remoting);
  AjaxRemotingProvider=Runtime.Safe(Remoting.AjaxRemotingProvider);
  List=Runtime.Safe(Global.WebSharper.List);
  Html=Runtime.Safe(Global.WebSharper.Html);
  Client1=Runtime.Safe(Html.Client);
  Tags=Runtime.Safe(Client1.Tags);
  Operators=Runtime.Safe(Client1.Operators);
  Attr=Runtime.Safe(Client1.Attr);
  T=Runtime.Safe(List.T);
  Concurrency=Runtime.Safe(Global.WebSharper.Concurrency);
  return setInterval=Runtime.Safe(Global.setInterval);
 });
 Runtime.OnLoad(function()
 {
  return;
 });
}());
