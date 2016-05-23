(function()
{
 var Global=this,Runtime=this.IntelliFactory.Runtime,MorningDashboard,Client,Remoting,AjaxRemotingProvider,Html,Client1,Operators,List,Tags,Attr,T,Concurrency,setInterval;
 Runtime.Define(Global,{
  MorningDashboard:{
   Client:{
    currentTimeBlock:function()
    {
     return Client.refreshBlock(5,function()
     {
      return AjaxRemotingProvider.Async("MorningDashboard:0",[]);
     },function(block)
     {
      return function(result)
      {
       var arg10,arg101,arg102,arg103,x,arg104,x1,arg105,x2;
       block["HtmlProvider@33"].Clear(block.get_Body());
       arg102=List.ofArray([Tags.Tags().text("Time")]);
       x=result.Time;
       arg103=List.ofArray([Tags.Tags().text(x)]);
       x1=result.Weekday;
       arg104=List.ofArray([Tags.Tags().text(x1)]);
       x2=result.Month+" "+result.Day;
       arg105=List.ofArray([Tags.Tags().text(x2)]);
       arg101=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg102),List.ofArray([Attr.Attr().NewAttr("class","panel-heading")])),Tags.Tags().NewTag("h2",arg103),Tags.Tags().NewTag("h4",arg104),Tags.Tags().NewTag("h4",arg105)]);
       arg10=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg101),List.ofArray([Attr.Attr().NewAttr("class","panel panel-default")]))]);
       return block.AppendI(Operators.add(Tags.Tags().NewTag("div",arg10),List.ofArray([Attr.Attr().NewAttr("class","col-md-6")])));
      };
     });
    },
    oneBusAwayBlock:function()
    {
     var updateCommuteBlock,getCommuteData;
     updateCommuteBlock=function(block)
     {
      return function(result)
      {
       var patternInput,routeTitle,arrivalStrings,mapping,arrivalElements,arg103,arg104,arg105,arg106,arg107,arg108,arg109,arg10a,arg10b;
       patternInput=[result.RouteTitle,result.Arrivals];
       routeTitle=patternInput[0];
       arrivalStrings=patternInput[1];
       mapping=function(arrival)
       {
        var arg10,arg101,x,arg102,x1;
        x=arrival.Time;
        arg101=List.ofArray([Tags.Tags().text(x)]);
        x1=arrival.TimeUntil;
        arg102=List.ofArray([Tags.Tags().text(x1)]);
        arg10=List.ofArray([Tags.Tags().NewTag("td",arg101),Tags.Tags().NewTag("td",arg102)]);
        return Tags.Tags().NewTag("tr",arg10);
       };
       arrivalElements=List.map(mapping,arrivalStrings);
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
     };
     getCommuteData=function()
     {
      return AjaxRemotingProvider.Async("MorningDashboard:2",[]);
     };
     return Client.refreshBlock(15,getCommuteData,updateCommuteBlock);
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
    },
    wundergroundBlock:function()
    {
     var updateBlock,getData;
     updateBlock=function(block)
     {
      return function(result)
      {
       var x,mapping,forecastElements,header,arg106,arg107,arg108,arg109,x3,arg10a,arg10b,arg10c,arg10d,arg10e,arg10f,arg1010,arg1011;
       x=result.Forecast;
       mapping=function(forecast)
       {
        var arg10,arg101,x1,arg102,arg103,arg104,arg105,x2;
        x1=forecast.Time;
        arg101=List.ofArray([Tags.Tags().text(x1)]);
        arg104="wi "+forecast.WeatherIcon;
        arg103=List.ofArray([Attr.Attr().NewAttr("class",arg104)]);
        arg102=List.ofArray([Tags.Tags().NewTag("i",arg103)]);
        x2=forecast.Temperature+"°";
        arg105=List.ofArray([Tags.Tags().text(x2)]);
        arg10=List.ofArray([Tags.Tags().NewTag("td",arg101),Tags.Tags().NewTag("td",arg102),Tags.Tags().NewTag("td",arg105)]);
        return Tags.Tags().NewTag("tr",arg10);
       };
       forecastElements=List.map(mapping,x);
       arg108="wi "+result.Current.WeatherIcon;
       arg107=List.ofArray([Attr.Attr().NewAttr("class",arg108)]);
       arg106=List.ofArray([Tags.Tags().NewTag("i",arg107)]);
       x3=result.Current.Temperature+"°";
       arg109=List.ofArray([Tags.Tags().text(x3)]);
       header=List.ofArray([Tags.Tags().NewTag("h1",arg106),Tags.Tags().NewTag("h4",arg109)]);
       block["HtmlProvider@33"].Clear(block.get_Body());
       arg10f=List.ofArray([Tags.Tags().text("Hour")]);
       arg1010=List.ofArray([Tags.Tags().text("Weather")]);
       arg1011=List.ofArray([Tags.Tags().text("Temperature")]);
       arg10e=List.ofArray([Tags.Tags().NewTag("th",arg10f),Tags.Tags().NewTag("th",arg1010),Tags.Tags().NewTag("th",arg1011)]);
       arg10d=List.ofArray([Tags.Tags().NewTag("tr",arg10e)]);
       arg10c=List.append(List.singleton(Tags.Tags().NewTag("thead",arg10d)),forecastElements);
       arg10b=List.ofArray([Operators.add(Tags.Tags().NewTag("div",header),List.ofArray([Attr.Attr().NewAttr("class","panel-heading")])),Operators.add(Tags.Tags().NewTag("table",arg10c),List.ofArray([Attr.Attr().NewAttr("class","table")]))]);
       arg10a=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg10b),List.ofArray([Attr.Attr().NewAttr("class","panel panel-default")]))]);
       return block.AppendI(Operators.add(Tags.Tags().NewTag("div",arg10a),List.ofArray([Attr.Attr().NewAttr("class","col-md-6")])));
      };
     };
     getData=function()
     {
      return AjaxRemotingProvider.Async("MorningDashboard:1",[]);
     };
     return Client.refreshBlock(60*15,getData,updateBlock);
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
  Html=Runtime.Safe(Global.WebSharper.Html);
  Client1=Runtime.Safe(Html.Client);
  Operators=Runtime.Safe(Client1.Operators);
  List=Runtime.Safe(Global.WebSharper.List);
  Tags=Runtime.Safe(Client1.Tags);
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
