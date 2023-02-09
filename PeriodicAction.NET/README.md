# Delay.NET

Platform Dependent Wait - Accurately wait down to `1ms` if your platform will allow it


```cs
using System.Threading;
```

```cs
Delay.Wait(1);
```

![VxBdU](https://user-images.githubusercontent.com/54571583/217691901-a6289ef7-408c-40b3-80fe-c40e70cda7ac.png)

# Note

If your platform doesn't support this it is the same as doing a `Thread.Sleep`