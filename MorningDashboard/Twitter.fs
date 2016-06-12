namespace MorningDashboard

open Newtonsoft.Json

module Twitter =
    type TwitterConfig = {
        ScreenName: string; 
        Keys: Tweetinvi.Core.Authentication.TwitterCredentials;
        JoinedScreenNames: string seq;}

    let replaceOptionWithEmpty (s: 'T seq option) = 
        match s with
        | Some x -> x
        | None -> Seq.empty<'T>

    let getCredentials (screenName:string) = 
        let keys = SharedCode.getKeyFile()
                    |> System.IO.File.ReadAllText
                    |> Linq.JObject.Parse
                    |> (fun k -> k.["TwitterConfig"].["Keys"])
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
    let userCache = SharedCode.makeNewCache<string,Tweetinvi.Core.Interfaces.IUser>()
    let friendCache = SharedCode.makeNewCache<string,Set<int64>>()

    let getUser (screenName:string) = 
        try
            Tweetinvi.TweetinviEvents.QueryBeforeExecute.Add( fun a -> a.TwitterQuery.Timeout <- System.TimeSpan.FromSeconds(30.0))
            let user = Tweetinvi.User.GetUserFromScreenName screenName
            if user = null then None else Some user
        with
        | exn -> None

    let getUserFromCache (screenName:string) =
        SharedCode.getFromCache userCache (60.0*60.0*6.0) getUser screenName

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
        Tweetinvi.User.GetFriendIds screenName
        |> Set.ofSeq
        |> Some
        with | _ -> None

    let getFriendsFromCache (screenName: string) =
        SharedCode.getFromCache friendCache (60.0*60.0*6.0) getFriends screenName   
         
    let getSharedFriendsIds (screenNames: string seq) =
        let usersAndFriends = 
            screenNames
                    |> Seq.map (fun screenName -> 
                                async {
                                    let result =
                                        match (getUserFromCache screenName) with
                                        | Some user ->
                                            match getFriendsFromCache user.ScreenName with
                                            | Some friends -> Some (user,friends)
                                            | None -> None
                                        | None -> None
                                    return result})
                    |> Async.Parallel
                    |> Async.RunSynchronously
                    |> Seq.ofArray
                    |> Seq.choose id
        
        let userIds = 
            usersAndFriends
            |> Seq.map fst
            |> Seq.map (fun user -> user.Id) 
            |> Set.ofSeq

        usersAndFriends
        |> Seq.map snd
        |> (fun s -> if Seq.length s = 0 then Set.empty<int64> else Set.intersectMany s)
        |> Set.union userIds


    let updateTweets (tweets: System.Collections.Generic.List<Tweetinvi.Core.Interfaces.ITweet>) = 
        tweets.RemoveAll(fun tweet -> tweet.CreatedAt.AddHours(6.0) < System.DateTime.Now)
        |> ignore

    let getTweetsFromUserAndJoinedFriends (screenName: string) (friends: Set<int64>) =
        let screenNameTweets = 
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
        screenNameTweets
        |> Seq.filter (fun tweet -> Set.contains tweet.CreatedBy.Id friends)

    let getTweetsFromConfig (config:TwitterConfig) =
        let friends = getSharedFriendsIds config.JoinedScreenNames
        getTweetsFromUserAndJoinedFriends config.ScreenName friends

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