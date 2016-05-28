namespace MorningDashboard

open Newtonsoft.Json

module Twitter =

    let replaceOptionWithEmpty (s: 'T seq option) = 
        match s with
        | Some x -> x
        | None -> Seq.empty<'T>

    let getCredentials (screenName:string) = 
        let keys = SharedCode.getKeyFile()
                    |> System.IO.File.ReadAllText
                    |> Linq.JObject.Parse
                    |> (fun k -> k.["TwitterKeys"]
                                    |> Seq.filter (fun user -> string user.["Name"] = screenName)
                                    |> Seq.head)
                    |> (fun k -> k.["Keys"])
        let consumerKey = string keys.["ConsumerKey"] 
        let consumerSecret = string keys.["ConsumerSecret"] 
        let accessToken = string keys.["AccessToken"] 
        let accessTokenSecret = string keys.["AccessTokenSecret"] 
        new Tweetinvi.Core.Authentication.TwitterCredentials(consumerKey,consumerSecret,accessToken,accessTokenSecret)



    type TweetData = {
        mutable MostRecentRequest: System.DateTimeOffset;
        mutable Tweets: System.Collections.Generic.List<Tweetinvi.Core.Interfaces.ITweet>;
        mutable Stream: Tweetinvi.Core.Interfaces.Streaminvi.IUserStream}
    
    let tweets = new System.Collections.Generic.Dictionary<string,TweetData>()

    let getUser (username:string) = 
        try
            Tweetinvi.TweetinviEvents.QueryBeforeExecute.Add( fun a -> a.TwitterQuery.Timeout <- System.TimeSpan.FromSeconds(30.0))
            let user = Tweetinvi.User.GetUserFromScreenName username
            if user = null then None else Some user
        with
        | exn -> 
            do System.Diagnostics.Debug.WriteLine("Couldn't pull tweets " + (Tweetinvi.ExceptionHandler.GetLastException()).TwitterDescription) 
            None

    let tweetText (x:Tweetinvi.Core.Interfaces.ITweet) = 
        let substitute (pattern:string) (replacement:string) (input:string) =
            let rgx = new System.Text.RegularExpressions.Regex(pattern)
            rgx.Replace(input,replacement)
        x.Text
        |> System.Web.HttpUtility.HtmlDecode 
        |> (substitute "\\s+" " ")
        |> (substitute "https{0,1}://t.co/\\S*" "")
        |> (fun y -> y.Trim(' ').TrimEnd(' '))

    let getTweets (username:string) = 
        let parameters = 
            let temp = new Tweetinvi.Core.Parameters.UserTimelineParameters()
            temp.IncludeRTS <- false
            temp.IncludeContributorDetails <- false
            temp.ExcludeReplies <- true
            temp.TrimUser <- true
            temp.MaximumNumberOfTweetsToRetrieve <- 100
            temp
        Tweetinvi.TweetinviEvents.QueryBeforeExecute.Add( fun a -> a.TwitterQuery.Timeout <- System.TimeSpan.FromSeconds(60.0))
        try Tweetinvi.Timeline.GetUserTimeline(username, parameters)
        with
        | exn -> 
            do System.Diagnostics.Debug.WriteLine("Couldn't pull tweets " + (Tweetinvi.ExceptionHandler.GetLastException()).TwitterDescription) 
            Seq.empty<Tweetinvi.Core.Interfaces.ITweet>

    let getFriends (screenName: string) =
        try 
        Tweetinvi.User.GetFriends screenName
        with | _ -> Seq.empty<Tweetinvi.Core.Interfaces.IUser>
                        
    let getSharedFriendsIds (screenNames: Set<string>) =
        let users = screenNames
                    |> Set.toSeq
                    |> Seq.choose getUser
        
        let userIds = users |> Seq.map (fun friend -> friend.Id) |> Set.ofSeq
        users
        |> Seq.map (fun user ->
                    async {
                        let friends = user.ScreenName |> getFriends |> Seq.map (fun friend -> friend.Id) |> Set.ofSeq
                        return Set.union friends userIds
                        }
                    )
        |> Async.Parallel
        |> Async.RunSynchronously
        |> Set.intersectMany

    let updateStream (screenNames: Set<string>) (stream:Tweetinvi.Core.Interfaces.Streaminvi.IFilteredStream) =
        do stream.ClearFollows()
        do screenNames
            |> getSharedFriendsIds
            |> Set.toSeq
            |> SharedCode.seqTopN 5
            |> Seq.iter (fun friendId -> stream.AddFollow(System.Nullable friendId))

    let updateTweets (tweets: System.Collections.Generic.List<Tweetinvi.Core.Interfaces.ITweet>) = 
        tweets.RemoveAll(fun tweet -> tweet.CreatedAt.AddHours(6.0) < System.DateTime.Now)
        |> ignore

    let getTweetsFromFriendsOfUser (screenName: string) =
        if tweets.ContainsKey screenName then
            System.Diagnostics.Debug.Write "Getting tweets from existing stream\n"
            let data = tweets.Item screenName
            do data.MostRecentRequest <- System.DateTimeOffset.Now
            data.Tweets
        else
            System.Diagnostics.Debug.Write "Creating new twitter stream\n"
            let mutable tweetData = {
                MostRecentRequest = System.DateTimeOffset.Now; 
                Stream = Tweetinvi.Stream.CreateUserStream(); 
                Tweets = new System.Collections.Generic.List<Tweetinvi.Core.Interfaces.ITweet>()
                }
            tweets.Add(screenName,tweetData)
            
            let eventFunction (eventArgs:Tweetinvi.Core.Events.EventArguments.TweetReceivedEventArgs) : unit =
                if not eventArgs.Tweet.IsRetweet && not eventArgs.Tweet.InReplyToUserId.HasValue then
                    tweetData.Tweets.Add(eventArgs.Tweet)
                    System.Diagnostics.Debug.Write ("Noticed a tweet: " + eventArgs.Tweet.Text + "\n")
            do tweetData.Stream.TweetCreatedByFriend.Add(eventFunction)
            do tweetData.Stream.TweetCreatedByMe.Add(eventFunction)
            do async { 
                    System.Diagnostics.Debug.Write "Prepping to start twitter stream\n"
                    let credentials = getCredentials screenName
                    do tweetData.Stream.Credentials <- credentials
                    do tweetData.Stream.StartStream()
                    return ()
                    }
            |> Async.Start
            |> ignore
            System.Diagnostics.Debug.Write "Twitter listener successfully started!\n"
            tweetData.Tweets

    let refreshFunction (eventArgs: System.Timers.ElapsedEventArgs) =
        tweets
        |> Seq.iter (fun row ->
            if row.Value.MostRecentRequest.AddHours(6.0) < System.DateTimeOffset.Now then
                do row.Value.Stream.StopStream()
                tweets.Remove(row.Key) |> ignore
            else
                //updateStream row.Key row.Value.Stream
                updateTweets row.Value.Tweets)

    let refreshTimer =
        let timer = new System.Timers.Timer(1000.0*60.0*60.0*24.0)
        do timer.AutoReset <- true
        do timer.Elapsed.Add refreshFunction
        do timer.Start()
        timer