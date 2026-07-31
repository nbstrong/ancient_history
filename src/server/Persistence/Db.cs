namespace WurmStyleGame.Server.Persistence;

public interface ITransactionRunner
{
    Task RunInTransaction(Func<Task> action, CancellationToken cancellationToken = default);
}

public sealed class InMemoryTransactionRunner : ITransactionRunner
{
    public Task RunInTransaction(Func<Task> action, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return action();
    }
}
