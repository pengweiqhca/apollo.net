using Com.Ctrip.Framework.Apollo.Core.Utils;
using Com.Ctrip.Framework.Apollo.Logging;

namespace Com.Ctrip.Framework.Apollo.Internals;

internal abstract class AbstractConfigRepository : IConfigRepository
{
    private static readonly Func<Action<LogLevel, FormattableString, Exception?>> Logger = static () =>
        LogManager.CreateLogger(typeof(AbstractConfigRepository));

    private readonly List<IRepositoryChangeListener> _listeners = new();
    private readonly SemaphoreSlim _waitHandle = new(1, 1);

    public string Namespace { get; }

    protected AbstractConfigRepository(string @namespace) => Namespace = @namespace;

    public abstract Properties GetConfig();

    public abstract Task Initialize();

    public void AddChangeListener(IRepositoryChangeListener listener)
    {
        _waitHandle.Wait();

        try
        {
            if (!_listeners.Contains(listener))
                _listeners.Add(listener);
        }
        finally
        {
            _waitHandle.Release();
        }
    }

    public void RemoveChangeListener(IRepositoryChangeListener listener)
    {
        _waitHandle.Wait();

        try
        {
            _listeners.Remove(listener);
        }
        finally
        {
            _waitHandle.Release();
        }
    }

    protected async Task FireRepositoryChange(string namespaceName, Properties newProperties)
    {
        await _waitHandle.WaitAsync().ConfigureAwait(false);

        try
        {
            await Task.WhenAll(_listeners.Select(listener =>
                listener.OnRepositoryChange(namespaceName, newProperties))).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger().Error($"Failed to invoke repository change listener.", ex);
        }
        finally
        {
            _waitHandle.Release();
        }
    }

    #region Dispose

    public void Dispose()
    {
        _waitHandle.Dispose();

        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected abstract void Dispose(bool disposing);

    ~AbstractConfigRepository() => Dispose(false);

    #endregion
}
