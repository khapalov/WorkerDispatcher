# WorkerDispatcher
Easy way to run background worker with various mode

## How to install

Install-Package WorkerDispatcher

<strong>Batch extension</strong><br/>
Install-Package WorkerDispatcher.Batch

## How to use
Dispatcher support three work mode Parallel,Sequenced,ParallelLimit<br/>
Parallel -> all tasks are called in parallel<br/>
Sequenced -> all tasks are called sequentially<br/>
ParallelLimit -> all tasks are called in parallel with the restriction on the number of threads (use PrefetchCount)<br/>

#### Create factory
For handle all result worker, create handler and pass into constructor ActionDispatcherFactory
```csharp
var factory = new ActionDispatcherFactory();
```

#### Start thread workers, and save result factory.Start() for usage in application, in the example it is variable "dispathcerToken"  
For change mode or change other settings, pass parameter in Start method

```csharp
var dispathcerToken = factory.Start();
```

#### Post some workers
```csharp

//inline post
dispatcherToken.Post(ct => Task.Delay(10000, ct));

//send some instanse MyWorker, need implement IActionInvoker
dispathcerToken.Post(new MyWorker());

//send some instanse MyDataWorker and pass MyTData, need implement IActionInvoker<TData>
dispathcerToken.Post(new MyDataWorker(), new MyTData());

//set lifetime for current post
dispathcerToken.Post(new MyDataWorker(), new MyTData(), TimeSpan.FromSecond(10));

//send inline post
dispatcherToken.Post(ct => Task.Delay(10000, ct));
```
### Use Batch
if you need to work with bach data, easy way add package WorkerDispatcher.Batch, and see example

```csharp
static void Main(string[] args)
{
    var factory = new ActionDispatcherFactory();
    var dispatcher = factory.Start();
    
    var bathToken = dispatcher.Plugin.Batch(p =>
    {
        p.For<SomeClass>()            
            //Max batch count
            .MaxCount(15)
            //Period afer 10 seccond, call IBatchActionInvoker
            .Period(TimeSpan.FromSeconds(10))
            //if 5 and more item added, call IBatchActionInvoker
            .TriggerCount(5)
            //If call stop app (bathToken.Stop()) true call IBatchActionInvoker and pass other items, else nothing
            .FlushOnStop(false)
            .Bind(() =>
            {
                return new BatchDataWorker();
            });   
    }).Start();

    //batchToken need save, for send batch data
    bathToken.Send(new SomeClass(1));
    bathToken.Send(new SomeClass(2));
    bathToken.Send(new SomeClass(3));

    Console.ReadKey();
    bathToken.Stop();
    bathToken.Dispose();
}
            
class BatchDataWorker : IBatchActionInvoker<SomeClass>
{
    public Task<object> Invoke(SomeClass[] data, CancellationToken token)
    {
        foreach (var d in data)
        {
            Console.WriteLine(d);
        }        
        return Task.FromResult(new object());    
    }
}
class SomeClass
{
    private readonly int _data;
    
    public SomeClass(int data)
    {
        _data = data;
    }
    public override string ToString()
    {
        return _data.ToString();
    }
}
```

### Run chain workers
Chain result available through, callback, synchronously, asynchronously, or call result in new worker, in example used callback

```csharp
dispatcherToken.Chain()
.Post(new MyDataWorker(), new MyTData())
.Post(new MyDataWorker(), new MyTData(), TimeSpan.FromSecond(10));
.Run(res =>
{
    //all worker result in "res"
});
```

#### Stop receive new worker and wait for the rest, default 60 second
```csharp
dispatcherToken.WaitCompleted();
dispatcherToken.Dispose();
```
