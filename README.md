# WorkerDispatcher
Easy to start async workers in your application

## How to use:
Dispatcher support three work mode Parallel,Sequenced,ParallelLimit
Parallel -> all tasks are called in parallel
Sequenced -> all tasks are called sequentially,
ParallelLimit -> all tasks are called in parallel with the restriction on the number of threads

## Default dispatcher mode, Parallel

All tasks are called in parallel

```csharp
var factory = new ActionDispatcherFactory();

//get and save dispatcher token
var dispathcerToken = factory.Start(new ActionDispatcherSettings
{
	//life time workers (2 minute)
	Timeout = TimeSpan.FromSeconds(120)
});

for (int i = 1; i <= 20; i++)
{
	//inline post worker
	dispathcerToken.Post(ct=> Task.Delay(100, ct));
}

//send some class MyWorker, need implement IActionInvoker
dispathcerToken.Post(new MyWorker());

//stop receive new worker and wait for the rest
await dispatcherToken.Stop();
```

## Next dispatcher mode, Sequenced

All tasks are called sequentially, Schedule = ScheduleType.Sequenced

```csharp
var factory = new ActionDispatcherFactory();

//get and save dispatcher token
var dispathcerToken = factory.Start(new ActionDispatcherSettings
{
	//life time workers (2 minute)
	Timeout = TimeSpan.FromSeconds(120),
	Schedule = ScheduleType.Sequenced
});

for (int i = 1; i <= 20; i++)
{
	//inline post worker
	dispathcerToken.Post(ct=> Task.Delay(100, ct));
}

//send some class MyWorker, need implement IActionInvoker
dispathcerToken.Post(new MyWorker());

//stop receive new worker and wait for the rest
await dispatcherToken.Stop();
```

## Next dispatcher mode, ParallelLimit

With three parallel tasks, PrefetchCount=3, Schedule = ScheduleType.ParallelLimit

```csharp
var factory = new ActionDispatcherFactory();

//get and save dispatcher token
var dispathcerToken = factory.Start(new ActionDispatcherSettings
{
	//life time workers (2 minute)
	Timeout = TimeSpan.FromSeconds(120),
	Schedule = ScheduleType.ParallelLimit,
	PrefetchCount = 3	
});

for (int i = 1; i <= 20; i++)
{
	//inline post worker
	dispathcerToken.Post(ct=> Task.Delay(100, ct));
}

//send some class MyWorker, need implement IActionInvoker
dispathcerToken.Post(new MyWorker());

//stop receive new worker and wait for the rest
await dispatcherToken.Stop();
```

