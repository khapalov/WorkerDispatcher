# WorkerDispatcher
Easy to start async workers in your application

## How to use:
Dispatcher support three work mode Parallel,Sequenced,ParallelLimit<br/>
Parallel -> all tasks are called in parallel<br/>
Sequenced -> all tasks are called sequentially<br/>
ParallelLimit -> all tasks are called in parallel with the restriction on the number of threads (use PrfecthCount)<br/>

```csharp
var handler = new MyDispatcherHandler();
var factory = new ActionDispatcherFactory(handler);

//get and save dispatcher token
var dispathcerToken = factory.Start(new ActionDispatcherSettings
{
	//life time workers (2 minute)
	Timeout = TimeSpan.FromSeconds(120),
	//Schedule - default Parallel
	Schedule = ScheduleType.ParallelLimit,
	//number of concurrent threads 
	PrfecthCount = 5
});

for (int i = 1; i <= 20; i++)
{	
	//send some class MyWorker, need implement IActionInvoker
	dispathcerToken.Post(new MyWorker());

	//send some class MyWorker, need implement IActionInvoker<TData>
	dispathcerToken.Post(new MyDataWorker<TData>(), new MyTData());

	//Set lifetime for current post
	dispathcerToken.Post(new MyDataWorker<TData>(), new MyTData(), TimeSpan.FromSecond(10));

	//Send inline post
	dispatcherToken.Post(ct => Task.Delay(10000, ct));
}

//stop receive new worker and wait for the rest, default 30 second
await dispatcherToken.Stop();
```