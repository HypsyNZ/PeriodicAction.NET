[![nuget-download-blue](https://user-images.githubusercontent.com/54571583/217700276-a8730c9a-c96e-47bd-a63a-d4a2874b572a.png)](https://www.nuget.org/packages/PeriodicAction.NET/)

# PeriodicAction.NET

Run an `Action` periodically at the `Interval` until it is canceled with a `CancellationTokenSource`


```cs
    CancellationTokenSource cts = new CancellationTokenSource();
    
    PeriodicAction Action = new PeriodicAction(() =>
    {
      // Your Action
    }, 1);
        
    Action.Run(cts.Token);
    cts.Cancel();
```