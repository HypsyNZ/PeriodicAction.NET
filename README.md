[![nuget-download-blue](https://user-images.githubusercontent.com/54571583/217700276-a8730c9a-c96e-47bd-a63a-d4a2874b572a.png)](https://www.nuget.org/packages/PeriodicAction.NET/)

# PeriodicAction.NET

Run an `Action` periodically at the `Interval` until it is canceled with a `CancellationTokenSource`


```cs
    CancellationTokenSource cts = new CancellationTokenSource();
    
    PeriodicAction Action = new PeriodicAction(() =>
    {
      // Your Action
    }, 1);
        
    Action.Run(cts.Token, ErrorStrategy.Ignore, TaskCreationOptions.DenyChildAttach);
    cts.Cancel();
```

# Error Strategy

| Strategy | Description |
|----|----|
| Ignore | Your work will silently fail, The Action will run again at the next Interval like it didn't happen.|
| Throw| Throw to Caller, This could crash your application if you don't catch it.|
| Event| This is the same as Ignoring Exceptions but the Exception will be forwarded to the ExceptionHandler so you can log it.


# Delay.NET

This also includes [Delay.NET](https://github.com/HypsyNZ/Delay.NET) so you don't need both.