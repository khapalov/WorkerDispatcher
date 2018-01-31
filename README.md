# WorkerDispatcher
Easy to start async workers in your application

### How to use:

```csharp
var factory = new ActionDispatcherFactory();

//get and save dispatcher token
var dispathcerToken = factory.Start(new ActionDispatcherSettings
{
	//life time workers (2 minute)
	Timeout = TimeSpan.FromSeconds(120)
});

//inline post worker
dispathcerToken.Post(ct=> Task.Delay(100, ct));

//send some class MyWorker, need implement IActionInvoker
dispathcerToken.Post(new MyWorker());

//stop receive new worker and wait for the rest
dispathcerToken.Stop();
```


