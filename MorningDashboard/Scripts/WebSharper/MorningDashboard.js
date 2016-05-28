(function()
{
 var Global=this,Runtime=this.IntelliFactory.Runtime,List,Html,Client,Tags,Operators,Attr,Seq,MorningDashboard,Client1,Remoting,AjaxRemotingProvider,T,Concurrency,setInterval;
 Runtime.Define(Global,{
  MorningDashboard:{
   Client:{
    calendarBlock:function()
    {
     var updateBlock,getData;
     updateBlock=function(block)
     {
      return function(result)
      {
       var x,mapping,lists,calendarElements,arg105,arg106,arg107,arg108;
       x=result.Calendars;
       mapping=function(calendar)
       {
        var x1,mapping1,instanceElements,arg103,arg104,x4;
        x1=calendar.Instances;
        mapping1=function(instance)
        {
         var arg10,arg101,x2,arg102,x3;
         x2=instance.Event;
         arg101=List.ofArray([Tags.Tags().text(x2)]);
         x3=instance.Time;
         arg102=List.ofArray([Tags.Tags().text(x3)]);
         arg10=List.ofArray([Tags.Tags().NewTag("td",arg101),Tags.Tags().NewTag("td",arg102)]);
         return Tags.Tags().NewTag("tr",arg10);
        };
        instanceElements=List.map(mapping1,x1);
        x4=calendar.Name;
        arg104=List.ofArray([Tags.Tags().text(x4)]);
        arg103=List.ofArray([Tags.Tags().NewTag("h5",arg104)]);
        return List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg103),List.ofArray([Attr.Attr().NewAttr("class","panel-body")])),Seq.length(calendar.Instances)>0?Operators.add(Tags.Tags().NewTag("table",instanceElements),List.ofArray([Attr.Attr().NewAttr("class","table")])):Client1.emptyTable("No events today")]);
       };
       lists=List.map(mapping,x);
       calendarElements=List.concat(lists);
       block["HtmlProvider@33"].Clear(block.get_Body());
       arg108=List.ofArray([Tags.Tags().text("Daily events")]);
       arg107=List.ofArray([Tags.Tags().NewTag("h4",arg108)]);
       arg106=List.append(List.singleton(Operators.add(Tags.Tags().NewTag("div",arg107),List.ofArray([Attr.Attr().NewAttr("class","panel-heading")]))),calendarElements);
       arg105=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg106),List.ofArray([Attr.Attr().NewAttr("class","panel panel-default")]))]);
       return block.AppendI(Operators.add(Tags.Tags().NewTag("div",arg105),List.ofArray([Attr.Attr().NewAttr("class","col-md-5")])));
      };
     };
     getData=function()
     {
      return AjaxRemotingProvider.Async("MorningDashboard:1",[]);
     };
     return Client1.refreshBlock(15*60,getData,updateBlock);
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
       return block.AppendI(Operators.add(Tags.Tags().NewTag("div",arg10),List.ofArray([Attr.Attr().NewAttr("class","col-md-5")])));
      };
     };
     getData=function()
     {
      return AjaxRemotingProvider.Async("MorningDashboard:2",[]);
     };
     return Client1.refreshBlock(5,getData,updateBlock);
    },
    emptyTable:function(message)
    {
     var arg10,arg101,arg102;
     arg102=List.ofArray([Tags.Tags().text(message)]);
     arg101=List.ofArray([Tags.Tags().NewTag("td",arg102)]);
     arg10=List.ofArray([Tags.Tags().NewTag("tr",arg101)]);
     return Operators.add(Tags.Tags().NewTag("table",arg10),List.ofArray([Attr.Attr().NewAttr("class","table")]));
    },
    oneBusAwayBlock:function()
    {
     var updateCommuteBlock,getCommuteData;
     updateCommuteBlock=function(block)
     {
      return function(resultCommutes)
      {
       var mapping,lists,resultBlocks,arg105,arg106,arg107,arg108;
       mapping=function(result)
       {
        var patternInput,routeTitle,arrivalStrings,mapping1,arrivalElements,arg103,arg104;
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
        return List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg103),List.ofArray([Attr.Attr().NewAttr("class","panel-body")])),Seq.length(arrivalStrings)>0?Operators.add(Tags.Tags().NewTag("table",arrivalElements),List.ofArray([Attr.Attr().NewAttr("class","table")])):Client1.emptyTable("No upcoming arrivals")]);
       };
       lists=List.map(mapping,resultCommutes);
       resultBlocks=List.concat(lists);
       block["HtmlProvider@33"].Clear(block.get_Body());
       arg108=List.ofArray([Tags.Tags().text("Commute")]);
       arg107=List.ofArray([Tags.Tags().NewTag("h4",arg108)]);
       arg106=List.append(List.singleton(Operators.add(Tags.Tags().NewTag("div",arg107),List.ofArray([Attr.Attr().NewAttr("class","panel-heading")]))),resultBlocks);
       arg105=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg106),List.ofArray([Attr.Attr().NewAttr("class","panel panel-default")]))]);
       return block.AppendI(Operators.add(Tags.Tags().NewTag("div",arg105),List.ofArray([Attr.Attr().NewAttr("class","col-md-3")])));
      };
     };
     getCommuteData=function()
     {
      return AjaxRemotingProvider.Async("MorningDashboard:4",[]);
     };
     return Client1.refreshBlock(5,getCommuteData,updateCommuteBlock);
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
    twitterBlock:function()
    {
     return Client1.refreshBlock(10,function()
     {
      return AjaxRemotingProvider.Async("MorningDashboard:0",[]);
     },function(block)
     {
      return function(result)
      {
       var x,tweetElements,tweetElements1,arg102,arg103,x2,arg104,arg105,arg106,arg107;
       x=result.Tweets;
       tweetElements=List.map(function(tweet)
       {
        var arg10,arg101,x1;
        x1=tweet.Username+": "+tweet.Text;
        arg101=List.ofArray([Tags.Tags().text(x1)]);
        arg10=List.ofArray([Tags.Tags().NewTag("td",arg101)]);
        return Tags.Tags().NewTag("tr",arg10);
       },x);
       x2=result.Title;
       arg103=List.ofArray([Tags.Tags().text(x2)]);
       arg102=List.ofArray([Tags.Tags().NewTag("h5",arg103)]);
       tweetElements1=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg102),List.ofArray([Attr.Attr().NewAttr("class","panel-body")])),Seq.length(tweetElements)>0?Operators.add(Tags.Tags().NewTag("table",tweetElements),List.ofArray([Attr.Attr().NewAttr("class","table table-condensed")])):Client1.emptyTable("No tweets available")]);
       block["HtmlProvider@33"].Clear(block.get_Body());
       arg107=List.ofArray([Tags.Tags().text("Recent Tweets")]);
       arg106=List.ofArray([Tags.Tags().NewTag("h4",arg107)]);
       arg105=List.append(List.singleton(Operators.add(Tags.Tags().NewTag("div",arg106),List.ofArray([Attr.Attr().NewAttr("class","panel-heading")]))),tweetElements1);
       arg104=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg105),List.ofArray([Attr.Attr().NewAttr("class","panel panel-default")]))]);
       return block.AppendI(Operators.add(Tags.Tags().NewTag("div",arg104),List.ofArray([Attr.Attr().NewAttr("class","col-md-5")])));
      };
     });
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
       return block.AppendI(Operators.add(Tags.Tags().NewTag("div",arg10e),List.ofArray([Attr.Attr().NewAttr("class","col-md-4")])));
      };
     };
     getData=function()
     {
      return AjaxRemotingProvider.Async("MorningDashboard:3",[]);
     };
     return Client1.refreshBlock(60*15,getData,updateBlock);
    }
   }
  }
 });
 Runtime.OnInit(function()
 {
  List=Runtime.Safe(Global.WebSharper.List);
  Html=Runtime.Safe(Global.WebSharper.Html);
  Client=Runtime.Safe(Html.Client);
  Tags=Runtime.Safe(Client.Tags);
  Operators=Runtime.Safe(Client.Operators);
  Attr=Runtime.Safe(Client.Attr);
  Seq=Runtime.Safe(Global.WebSharper.Seq);
  MorningDashboard=Runtime.Safe(Global.MorningDashboard);
  Client1=Runtime.Safe(MorningDashboard.Client);
  Remoting=Runtime.Safe(Global.WebSharper.Remoting);
  AjaxRemotingProvider=Runtime.Safe(Remoting.AjaxRemotingProvider);
  T=Runtime.Safe(List.T);
  Concurrency=Runtime.Safe(Global.WebSharper.Concurrency);
  return setInterval=Runtime.Safe(Global.setInterval);
 });
 Runtime.OnLoad(function()
 {
  return;
 });
}());
