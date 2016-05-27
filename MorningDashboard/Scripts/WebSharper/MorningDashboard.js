(function()
{
 var Global=this,Runtime=this.IntelliFactory.Runtime,MorningDashboard,Client,Remoting,AjaxRemotingProvider,List,Html,Client1,Tags,Operators,Attr,Seq,T,Concurrency,setInterval;
 Runtime.Define(Global,{
  MorningDashboard:{
   Client:{
    calendarBlock:function()
    {
     return Client.refreshBlock(15*60,function()
     {
      return AjaxRemotingProvider.Async("MorningDashboard:0",[]);
     },function(block)
     {
      return function(result)
      {
       var x,x1,calendarElements,arg105,arg106,arg107,arg108;
       x=result.Calendars;
       x1=List.map(function(calendar)
       {
        var x2,instanceElements,arg103,arg104,x5;
        x2=calendar.Instances;
        instanceElements=List.map(function(instance)
        {
         var arg10,arg101,x3,arg102,x4;
         x3=instance.Event;
         arg101=List.ofArray([Tags.Tags().text(x3)]);
         x4=instance.Time;
         arg102=List.ofArray([Tags.Tags().text(x4)]);
         arg10=List.ofArray([Tags.Tags().NewTag("td",arg101),Tags.Tags().NewTag("td",arg102)]);
         return Tags.Tags().NewTag("tr",arg10);
        },x2);
        x5=calendar.Name;
        arg104=List.ofArray([Tags.Tags().text(x5)]);
        arg103=List.ofArray([Tags.Tags().NewTag("h5",arg104)]);
        return List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg103),List.ofArray([Attr.Attr().NewAttr("class","panel-body")])),Operators.add(Tags.Tags().NewTag("table",instanceElements),List.ofArray([Attr.Attr().NewAttr("class","table")]))]);
       },x);
       calendarElements=List.concat(x1);
       block["HtmlProvider@33"].Clear(block.get_Body());
       arg108=List.ofArray([Tags.Tags().text("Daily events")]);
       arg107=List.ofArray([Tags.Tags().NewTag("h4",arg108)]);
       arg106=List.append(List.singleton(Operators.add(Tags.Tags().NewTag("div",arg107),List.ofArray([Attr.Attr().NewAttr("class","panel-heading")]))),calendarElements);
       arg105=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg106),List.ofArray([Attr.Attr().NewAttr("class","panel panel-default")]))]);
       return block.AppendI(Operators.add(Tags.Tags().NewTag("div",arg105),List.ofArray([Attr.Attr().NewAttr("class","col-md-6")])));
      };
     });
    },
    currentTimeBlock:function()
    {
     var updateBlock,getData;
     updateBlock=function(block)
     {
      return function(result)
      {
       var arg10,arg101,arg102,arg103,arg104,arg105,arg106,x,arg107,arg108,x1,arg109,x2;
       block["HtmlProvider@33"].Clear(block.get_Body());
       arg103=List.ofArray([Tags.Tags().text("Time and date")]);
       arg102=List.ofArray([Tags.Tags().NewTag("h4",arg103)]);
       x=result.Time;
       arg106=List.ofArray([Tags.Tags().text(x)]);
       arg105=List.ofArray([Operators.add(Tags.Tags().NewTag("h1",arg106),List.ofArray([Attr.Attr().NewAttr("class","highlight")]))]);
       x1=result.Weekday;
       arg108=List.ofArray([Tags.Tags().text(x1)]);
       x2=result.Month+" "+result.Day;
       arg109=List.ofArray([Tags.Tags().text(x2)]);
       arg107=List.ofArray([Tags.Tags().NewTag("h4",arg108),Tags.Tags().NewTag("h4",arg109)]);
       arg104=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg105),List.ofArray([Attr.Attr().NewAttr("class","col-md-6")])),Operators.add(Tags.Tags().NewTag("div",arg107),List.ofArray([Attr.Attr().NewAttr("class","col-md-6")]))]);
       arg101=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg102),List.ofArray([Attr.Attr().NewAttr("class","panel-heading")])),Operators.add(Tags.Tags().NewTag("div",arg104),List.ofArray([Attr.Attr().NewAttr("class","panel-body")]))]);
       arg10=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg101),List.ofArray([Attr.Attr().NewAttr("class","panel panel-default")]))]);
       return block.AppendI(Operators.add(Tags.Tags().NewTag("div",arg10),List.ofArray([Attr.Attr().NewAttr("class","col-md-6")])));
      };
     };
     getData=function()
     {
      return AjaxRemotingProvider.Async("MorningDashboard:1",[]);
     };
     return Client.refreshBlock(5,getData,updateBlock);
    },
    oneBusAwayBlock:function()
    {
     var updateCommuteBlock,getCommuteData;
     updateCommuteBlock=function(block)
     {
      return function(resultCommutes)
      {
       var mapping,lists,resultBlocks,arg107,arg108,arg109,arg10a;
       mapping=function(result)
       {
        var patternInput,routeTitle,arrivalStrings,mapping1,arrivalElements,arg103,arg104,_,arg105,arg106;
        patternInput=[result.RouteTitle,result.Arrivals];
        routeTitle=patternInput[0];
        arrivalStrings=patternInput[1];
        mapping1=function(arrival)
        {
         var arg10,arg101,x,arg102,x1;
         x=arrival.Time;
         arg101=List.ofArray([Tags.Tags().text(x)]);
         x1=arrival.TimeUntil;
         arg102=List.ofArray([Tags.Tags().text(x1)]);
         arg10=List.ofArray([Tags.Tags().NewTag("td",arg101),Tags.Tags().NewTag("td",arg102)]);
         return Tags.Tags().NewTag("tr",arg10);
        };
        arrivalElements=List.map(mapping1,arrivalStrings);
        arg104=List.ofArray([Tags.Tags().text(routeTitle)]);
        arg103=List.ofArray([Tags.Tags().NewTag("h5",arg104)]);
        if(Seq.length(arrivalStrings)>0)
         {
          _=Operators.add(Tags.Tags().NewTag("table",arrivalElements),List.ofArray([Attr.Attr().NewAttr("class","table")]));
         }
        else
         {
          arg106=List.ofArray([Tags.Tags().text("No upcoming arrivals")]);
          arg105=List.ofArray([Operators.add(Tags.Tags().NewTag("li",arg106),List.ofArray([Attr.Attr().NewAttr("class","list-group-item")]))]);
          _=Operators.add(Tags.Tags().NewTag("ul",arg105),List.ofArray([Attr.Attr().NewAttr("class","list-group")]));
         }
        return List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg103),List.ofArray([Attr.Attr().NewAttr("class","panel-body")])),_]);
       };
       lists=List.map(mapping,resultCommutes);
       resultBlocks=List.concat(lists);
       block["HtmlProvider@33"].Clear(block.get_Body());
       arg10a=List.ofArray([Tags.Tags().text("Commute")]);
       arg109=List.ofArray([Tags.Tags().NewTag("h4",arg10a)]);
       arg108=List.append(List.singleton(Operators.add(Tags.Tags().NewTag("div",arg109),List.ofArray([Attr.Attr().NewAttr("class","panel-heading")]))),resultBlocks);
       arg107=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg108),List.ofArray([Attr.Attr().NewAttr("class","panel panel-default")]))]);
       return block.AppendI(Operators.add(Tags.Tags().NewTag("div",arg107),List.ofArray([Attr.Attr().NewAttr("class","col-md-6")])));
      };
     };
     getCommuteData=function()
     {
      return AjaxRemotingProvider.Async("MorningDashboard:3",[]);
     };
     return Client.refreshBlock(5,getCommuteData,updateCommuteBlock);
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
       var x,mapping,forecastElements,body,arg106,arg107,arg108,arg109,arg10a,arg10b,x3,arg10c,x4,arg10d,x5,arg10e,arg10f,arg1010,arg1011;
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
       arg109="weather wi "+result.Current.WeatherIcon;
       arg108=List.ofArray([Attr.Attr().NewAttr("class",arg109)]);
       arg107=List.ofArray([Tags.Tags().NewTag("i",arg108)]);
       arg106=List.ofArray([Operators.add(Tags.Tags().NewTag("h1",arg107),List.ofArray([Attr.Attr().NewAttr("class","highlight")]))]);
       x3="Current: "+result.Current.Temperature+"°";
       arg10b=List.ofArray([Tags.Tags().text(x3)]);
       x4="High: "+result.Current.High+"°";
       arg10c=List.ofArray([Tags.Tags().text(x4)]);
       x5="Low: "+result.Current.Low+"°";
       arg10d=List.ofArray([Tags.Tags().text(x5)]);
       arg10a=List.ofArray([Tags.Tags().NewTag("h4",arg10b),Tags.Tags().NewTag("h4",arg10c),Tags.Tags().NewTag("h4",arg10d)]);
       body=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg106),List.ofArray([Attr.Attr().NewAttr("class","col-md-6")])),Operators.add(Tags.Tags().NewTag("div",arg10a),List.ofArray([Attr.Attr().NewAttr("class","col-md-6")]))]);
       block["HtmlProvider@33"].Clear(block.get_Body());
       arg1011=List.ofArray([Tags.Tags().text("Weather")]);
       arg1010=List.ofArray([Tags.Tags().NewTag("h4",arg1011)]);
       arg10f=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg1010),List.ofArray([Attr.Attr().NewAttr("class","panel-heading")])),Operators.add(Tags.Tags().NewTag("div",body),List.ofArray([Attr.Attr().NewAttr("class","panel-body")])),Operators.add(Tags.Tags().NewTag("table",forecastElements),List.ofArray([Attr.Attr().NewAttr("class","table table-condensed")]))]);
       arg10e=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg10f),List.ofArray([Attr.Attr().NewAttr("class","panel panel-default")]))]);
       return block.AppendI(Operators.add(Tags.Tags().NewTag("div",arg10e),List.ofArray([Attr.Attr().NewAttr("class","col-md-6")])));
      };
     };
     getData=function()
     {
      return AjaxRemotingProvider.Async("MorningDashboard:2",[]);
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
  List=Runtime.Safe(Global.WebSharper.List);
  Html=Runtime.Safe(Global.WebSharper.Html);
  Client1=Runtime.Safe(Html.Client);
  Tags=Runtime.Safe(Client1.Tags);
  Operators=Runtime.Safe(Client1.Operators);
  Attr=Runtime.Safe(Client1.Attr);
  Seq=Runtime.Safe(Global.WebSharper.Seq);
  T=Runtime.Safe(List.T);
  Concurrency=Runtime.Safe(Global.WebSharper.Concurrency);
  return setInterval=Runtime.Safe(Global.setInterval);
 });
 Runtime.OnLoad(function()
 {
  return;
 });
}());
