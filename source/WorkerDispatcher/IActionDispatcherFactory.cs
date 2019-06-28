namespace WorkerDispatcher
{
    public interface IActionDispatcherFactory
    {
        IDispatcherToken Start(ActionDispatcherSettings config = null);
    }
}
