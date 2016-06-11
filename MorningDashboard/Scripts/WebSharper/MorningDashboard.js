(function()
{
 var Global=this,Runtime=this.IntelliFactory.Runtime,List,Html,Client,Operators,Tags,Attr,Seq,MorningDashboard,Client1,Remoting,AjaxRemotingProvider,T,Concurrency,setInterval,jQuery,window;
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
       var x,mapping,lists,calendarElements,arg105,arg106,arg107;
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
         arg10=List.ofArray([Operators.add(Tags.Tags().NewTag("td",arg101),List.ofArray([Attr.Attr().NewAttr("class","col-md-8")])),Operators.add(Tags.Tags().NewTag("td",arg102),List.ofArray([Attr.Attr().NewAttr("class","col-md-4")]))]);
         return Tags.Tags().NewTag("tr",arg10);
        };
        instanceElements=List.map(mapping1,x1);
        x4=calendar.Name;
        arg104=List.ofArray([Tags.Tags().text(x4)]);
        arg103=List.ofArray([Operators.add(Tags.Tags().NewTag("h5",arg104),List.ofArray([Attr.Attr().NewAttr("class","accent-text")]))]);
        return List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg103),List.ofArray([Attr.Attr().NewAttr("class","panel-body")])),Seq.length(calendar.Instances)>0?Operators.add(Tags.Tags().NewTag("table",instanceElements),List.ofArray([Attr.Attr().NewAttr("class","table")])):Client1.emptyTable("No events today")]);
       };
       lists=List.map(mapping,x);
       calendarElements=List.concat(lists);
       block["HtmlProvider@33"].Clear(block.get_Body());
       arg107=List.ofArray([Tags.Tags().text("Daily events")]);
       arg106=List.ofArray([Tags.Tags().NewTag("h4",arg107)]);
       arg105=List.append(List.singleton(Operators.add(Tags.Tags().NewTag("div",arg106),List.ofArray([Attr.Attr().NewAttr("class","panel-heading")]))),calendarElements);
       return block.AppendI(Operators.add(Tags.Tags().NewTag("div",arg105),List.ofArray([Attr.Attr().NewAttr("class","panel panel-default")])));
      };
     };
     getData=function()
     {
      return AjaxRemotingProvider.Async("MorningDashboard:1",[]);
     };
     return Client1.refreshBlock("calendarBlock",15*60,Runtime.New(T,{
      $:0
     }),getData,updateBlock);
    },
    currentTimeBlock:function()
    {
     var updateBlock,getData;
     updateBlock=function(block)
     {
      return function(result)
      {
       var arg10,arg101,arg102,x,arg103,x1,arg104,x2;
       block["HtmlProvider@33"].Clear(block.get_Body());
       x=result.Time;
       arg102=List.ofArray([Tags.Tags().text(x)]);
       x1=result.Weekday;
       arg103=List.ofArray([Tags.Tags().text(x1)]);
       x2=result.Month+" "+result.Day;
       arg104=List.ofArray([Tags.Tags().text(x2)]);
       arg101=List.ofArray([Operators.add(Tags.Tags().NewTag("h1",arg102),List.ofArray([Attr.Attr().NewAttr("class","accent-text highlight-small")])),Operators.add(Tags.Tags().NewTag("h4",arg103),List.ofArray([Attr.Attr().NewAttr("class","accent-text")])),Operators.add(Tags.Tags().NewTag("h4",arg104),List.ofArray([Attr.Attr().NewAttr("class","accent-text")]))]);
       arg10=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg101),List.ofArray([Attr.Attr().NewAttr("class","panel-body text-center")]))]);
       return block.AppendI(Operators.add(Tags.Tags().NewTag("div",arg10),List.ofArray([Attr.Attr().NewAttr("class","panel panel-default")])));
      };
     };
     getData=function()
     {
      return AjaxRemotingProvider.Async("MorningDashboard:2",[]);
     };
     return Client1.refreshBlock("currentTimeBlock",5,Runtime.New(T,{
      $:0
     }),getData,updateBlock);
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
       var mapping,lists,resultBlocks,arg105,arg106,arg107;
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
         arg10=List.ofArray([Operators.add(Tags.Tags().NewTag("td",arg101),List.ofArray([Attr.Attr().NewAttr("class","col-md-8")])),Operators.add(Tags.Tags().NewTag("td",arg102),List.ofArray([Attr.Attr().NewAttr("class","col-md-4")]))]);
         return Tags.Tags().NewTag("tr",arg10);
        };
        arrivalElements=List.map(mapping1,arrivalStrings);
        arg104=List.ofArray([Tags.Tags().text(routeTitle)]);
        arg103=List.ofArray([Operators.add(Tags.Tags().NewTag("h5",arg104),List.ofArray([Attr.Attr().NewAttr("class","accent-text")]))]);
        return List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg103),List.ofArray([Attr.Attr().NewAttr("class","panel-body")])),Seq.length(arrivalStrings)>0?Operators.add(Tags.Tags().NewTag("table",arrivalElements),List.ofArray([Attr.Attr().NewAttr("class","table")])):Client1.emptyTable("No upcoming arrivals")]);
       };
       lists=List.map(mapping,resultCommutes);
       resultBlocks=List.concat(lists);
       block["HtmlProvider@33"].Clear(block.get_Body());
       arg107=List.ofArray([Tags.Tags().text("Commute")]);
       arg106=List.ofArray([Tags.Tags().NewTag("h4",arg107)]);
       arg105=List.append(List.singleton(Operators.add(Tags.Tags().NewTag("div",arg106),List.ofArray([Attr.Attr().NewAttr("class","panel-heading")]))),resultBlocks);
       return block.AppendI(Operators.add(Tags.Tags().NewTag("div",arg105),List.ofArray([Attr.Attr().NewAttr("class","panel panel-default")])));
      };
     };
     getCommuteData=function()
     {
      return AjaxRemotingProvider.Async("MorningDashboard:4",[]);
     };
     return Client1.refreshBlock("oneBusAwayBlock",5,Runtime.New(T,{
      $:0
     }),getCommuteData,updateCommuteBlock);
    },
    refreshBlock:function(id,seconds,initialContents,getDataFunction,updateBlockFunction)
    {
     var output,repeater,f;
     output=Operators.add(Tags.Tags().NewTag("div",initialContents),List.ofArray([Attr.Attr().NewAttr("id",id)]));
     repeater=function(e)
     {
      var arg00;
      arg00=Concurrency.Delay(function()
      {
       return Concurrency.Bind(getDataFunction(null),function(_arg1)
       {
        var _,x;
        if(_arg1.$==1)
         {
          x=_arg1.$0;
          (updateBlockFunction(e))(x);
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
     return output;
    },
    trafficMapBlock:function()
    {
     var getData,arg10,arg101,arg102,arg103;
     getData=function()
     {
      return Concurrency.Delay(function()
      {
       return Concurrency.Return({
        $:1,
        $0:null
       });
      });
     };
     arg102=List.ofArray([Tags.Tags().text("Traffic")]);
     arg101=List.ofArray([Tags.Tags().NewTag("h4",arg102)]);
     arg103=List.ofArray([Attr.Attr().NewAttr("id","trafficMap"),Attr.Attr().NewAttr("class","map")]);
     arg10=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg101),List.ofArray([Attr.Attr().NewAttr("class","panel-heading")])),Tags.Tags().NewTag("div",arg103)]);
     return Client1.refreshBlock("trafficMapBlock",10*60,List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg10),List.ofArray([Attr.Attr().NewAttr("class","panel panel-default")]))]),getData,function()
     {
      return function()
      {
       return jQuery("trafficMap").ready(window.createMap.call(null,"trafficMap"));
      };
     });
    },
    twitterBlock:function()
    {
     var updateBlock,getData;
     updateBlock=function(block)
     {
      return function(result)
      {
       var x,mapping,tweetElements,tweetElements1,arg102,arg103,x2,arg104,arg105,arg106;
       x=result.Tweets;
       mapping=function(tweet)
       {
        var arg10,arg101,x1;
        x1=tweet.Username+": "+tweet.Text;
        arg101=List.ofArray([Tags.Tags().text(x1)]);
        arg10=List.ofArray([Tags.Tags().NewTag("td",arg101)]);
        return Tags.Tags().NewTag("tr",arg10);
       };
       tweetElements=List.map(mapping,x);
       x2=result.Title;
       arg103=List.ofArray([Tags.Tags().text(x2)]);
       arg102=List.ofArray([Operators.add(Tags.Tags().NewTag("h5",arg103),List.ofArray([Attr.Attr().NewAttr("class","accent-text")]))]);
       tweetElements1=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg102),List.ofArray([Attr.Attr().NewAttr("class","panel-body")])),Seq.length(tweetElements)>0?Operators.add(Tags.Tags().NewTag("table",tweetElements),List.ofArray([Attr.Attr().NewAttr("class","table table-condensed")])):Client1.emptyTable("No tweets available")]);
       block["HtmlProvider@33"].Clear(block.get_Body());
       arg106=List.ofArray([Tags.Tags().text("Recent Tweets")]);
       arg105=List.ofArray([Tags.Tags().NewTag("h4",arg106)]);
       arg104=List.append(List.singleton(Operators.add(Tags.Tags().NewTag("div",arg105),List.ofArray([Attr.Attr().NewAttr("class","panel-heading")]))),tweetElements1);
       return block.AppendI(Operators.add(Tags.Tags().NewTag("div",arg104),List.ofArray([Attr.Attr().NewAttr("class","panel panel-default")])));
      };
     };
     getData=function()
     {
      return AjaxRemotingProvider.Async("MorningDashboard:0",[]);
     };
     return Client1.refreshBlock("twitterBlock",10,Runtime.New(T,{
      $:0
     }),getData,updateBlock);
    },
    wundergroundBlock:function()
    {
     var updateBlock,getData;
     updateBlock=function(block)
     {
      return function(result)
      {
       var x,mapping,forecastElements,body,arg105,arg106,arg107,arg108,arg109,arg10a,x3,arg10b,x4,arg10c,x5,arg10d,arg10e,arg10f;
       x=result.Forecast;
       mapping=function(forecast)
       {
        var arg10,arg101,x1,arg102,arg103,arg104,x2;
        x1=forecast.Time;
        arg101=List.ofArray([Tags.Tags().text(x1)]);
        arg103=List.ofArray([Attr.Attr().NewAttr("class","wi "+forecast.WeatherIcon+(forecast.Accent?" accent-text-minor":""))]);
        arg102=List.ofArray([Tags.Tags().NewTag("i",arg103)]);
        x2=forecast.Temperature+"°";
        arg104=List.ofArray([Tags.Tags().text(x2)]);
        arg10=List.ofArray([Tags.Tags().NewTag("td",arg101),Tags.Tags().NewTag("td",arg102),Tags.Tags().NewTag("td",arg104)]);
        return Tags.Tags().NewTag("tr",arg10);
       };
       forecastElements=List.map(mapping,x);
       arg108="weather wi "+result.Current.WeatherIcon;
       arg107=List.ofArray([Attr.Attr().NewAttr("class",arg108)]);
       arg106=List.ofArray([Tags.Tags().NewTag("i",arg107)]);
       arg105=List.ofArray([Operators.add(Tags.Tags().NewTag("h1",arg106),List.ofArray([Attr.Attr().NewAttr("class","highlight "+(result.Current.Accent?"accent-text-minor":"accent-text"))]))]);
       x3="Now: "+result.Current.Temperature+"°";
       arg10a=List.ofArray([Tags.Tags().text(x3)]);
       x4="High: "+result.Current.High+"°";
       arg10b=List.ofArray([Tags.Tags().text(x4)]);
       x5="Low: "+result.Current.Low+"°";
       arg10c=List.ofArray([Tags.Tags().text(x5)]);
       arg109=List.ofArray([Operators.add(Tags.Tags().NewTag("h4",arg10a),List.ofArray([Attr.Attr().NewAttr("class","accent-text")])),Operators.add(Tags.Tags().NewTag("h4",arg10b),List.ofArray([Attr.Attr().NewAttr("class","accent-text")])),Operators.add(Tags.Tags().NewTag("h4",arg10c),List.ofArray([Attr.Attr().NewAttr("class","accent-text")]))]);
       body=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg105),List.ofArray([Attr.Attr().NewAttr("class","col-md-5 text-center")])),Operators.add(Tags.Tags().NewTag("div",arg109),List.ofArray([Attr.Attr().NewAttr("class","col-md-7")]))]);
       block["HtmlProvider@33"].Clear(block.get_Body());
       arg10f=List.ofArray([Tags.Tags().text("Weather")]);
       arg10e=List.ofArray([Tags.Tags().NewTag("h4",arg10f)]);
       arg10d=List.ofArray([Operators.add(Tags.Tags().NewTag("div",arg10e),List.ofArray([Attr.Attr().NewAttr("class","panel-heading")])),Operators.add(Tags.Tags().NewTag("div",body),List.ofArray([Attr.Attr().NewAttr("class","panel-body")])),Operators.add(Tags.Tags().NewTag("table",forecastElements),List.ofArray([Attr.Attr().NewAttr("class","table table-condensed")]))]);
       return block.AppendI(Operators.add(Tags.Tags().NewTag("div",arg10d),List.ofArray([Attr.Attr().NewAttr("class","panel panel-default")])));
      };
     };
     getData=function()
     {
      return AjaxRemotingProvider.Async("MorningDashboard:3",[]);
     };
     return Client1.refreshBlock("wundergroundBlock",60*15,Runtime.New(T,{
      $:0
     }),getData,updateBlock);
    }
   }
  }
 });
 Runtime.OnInit(function()
 {
  List=Runtime.Safe(Global.WebSharper.List);
  Html=Runtime.Safe(Global.WebSharper.Html);
  Client=Runtime.Safe(Html.Client);
  Operators=Runtime.Safe(Client.Operators);
  Tags=Runtime.Safe(Client.Tags);
  Attr=Runtime.Safe(Client.Attr);
  Seq=Runtime.Safe(Global.WebSharper.Seq);
  MorningDashboard=Runtime.Safe(Global.MorningDashboard);
  Client1=Runtime.Safe(MorningDashboard.Client);
  Remoting=Runtime.Safe(Global.WebSharper.Remoting);
  AjaxRemotingProvider=Runtime.Safe(Remoting.AjaxRemotingProvider);
  T=Runtime.Safe(List.T);
  Concurrency=Runtime.Safe(Global.WebSharper.Concurrency);
  setInterval=Runtime.Safe(Global.setInterval);
  jQuery=Runtime.Safe(Global.jQuery);
  return window=Runtime.Safe(Global.window);
 });
 Runtime.OnLoad(function()
 {
  return;
 });
}());
