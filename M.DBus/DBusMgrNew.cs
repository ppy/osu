using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using M.DBus.Utils;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using Tmds.DBus;

namespace M.DBus;

public partial class DBusMgrNew : CompositeDrawable
{
    private Connection? currentConnection;
    public ConnectionState ConnectionState { get; private set; } = ConnectionState.Disconnected;

    private string targetUrl = Address.Session;

    public string TargetURL
    {
        get => targetUrl;
        set => targetUrl = value;
    }

    public DBusMgrNew()
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Task.Run(StartConnect);
    }

    private readonly SemaphoreSlim writeLock = new SemaphoreSlim(1, 1);

    #region Connect and Disconnect

    private CancellationTokenSource? cancellationTokenSource;

    public void StartConnect()
    {
        Disconnect();

        this.cancellationTokenSource = new CancellationTokenSource();
        currentConnection = new Connection(new ClientConnectionOptions(TargetURL)
        {
            AutoConnect = false
        });

        Task.Run(startConnectTask, cancellationTokenSource.Token);
    }

    private async Task startConnectTask()
    {
        try
        {
            await writeLock.WaitAsync().ConfigureAwait(false);

            if (currentConnection == null)
                throw new NullDependencyException("Called StartConnect but DBusConnection is not ready!");

            this.currentConnection = new Connection(TargetURL);
            this.ConnectionState = ConnectionState.Connecting;

            // Await for connection to finish
            await currentConnection.ConnectAsync().ConfigureAwait(false);
            currentConnection.StateChanged += onConnectionStateChanged;

            this.ConnectionState = ConnectionState.Connected;

            // Resolve all previously registed objects to DBus
            foreach (var keyValuePair in registedObjects)
                await registerToConnectionTask(keyValuePair.Key).ConfigureAwait(false);

            OnConnected?.Invoke();
        }
        catch (Exception e)
        {
            this.ConnectionState = ConnectionState.Disconnected;
            Logger.Error(e, "初始化到DBus的连接时出现异常");
        }
        finally
        {
            writeLock.Release();
        }
    }

    private void onConnectionStateChanged(object? sender, ConnectionStateChangedEventArgs e)
    {
        Logger.Log($"-----------------------DBus Connection State Changed--------------------------");

        Logger.Log($"Sender: {sender}");
        Logger.Log($"Connection State: {e.State}");
        Logger.Log($"Reason: {e.DisconnectReason}");

        if (e.DisconnectReason != null)
            Logger.Log($"StackTrace: {e.DisconnectReason.StackTrace}");

        if (e.State == ConnectionState.Connected)
        {
            Logger.Log($"Info#LocalName: {e.ConnectionInfo.LocalName}");
            Logger.Log($"Info#RemoteIsBus: {e.ConnectionInfo.RemoteIsBus}");
        }

        Logger.Log($"-----------------------End State Report---------------------------------------");

        this.ConnectionState = e.State;
    }

    public void Disconnect()
    {
        cancellationTokenSource?.Cancel();

        currentConnection?.Dispose();
        currentConnection = null;
    }

    #endregion

    #region Object Register

    private readonly ConcurrentDictionary<IMDBusObject, string> registedObjects = new ConcurrentDictionary<IMDBusObject, string>();

    /// <summary>
    /// Register a dbus object to this manager.
    /// </summary>
    /// <param name="dBusObject">The target object to register</param>
    /// <returns></returns>
    public RegisterResult RegisterObject(IMDBusObject? dBusObject)
    {
        if (dBusObject == null) return RegisterResult.NULL_OBJECT;

        if (registedObjects.ContainsKey(dBusObject))
            return RegisterResult.PATH_ALREADY_IN_USE;

        string registedName = string.IsNullOrEmpty(dBusObject.CustomRegisterName)
            ? dBusObject.ObjectPath.ToServiceName()
            : dBusObject.CustomRegisterName;

        writeLock.Wait();

        registedObjects[dBusObject] = registedName;
        Task.Run(() => registerToConnectionTask(dBusObject));

        writeLock.Release();

        return RegisterResult.OK;
    }

    /// <summary>
    /// 将目标对象注册到DBus连接上
    /// <br/>
    /// 如果连接没准备好，则不会做任何事
    /// </summary>
    private async Task registerToConnectionTask(IMDBusObject obj)
    {
        if (!ConnectionReady())
            return;

        Debug.Assert(currentConnection != null, nameof(currentConnection) + " != null");
        await currentConnection.RegisterObjectAsync(obj).ConfigureAwait(false);

        string serviceName = registedObjects[obj];

        if (obj.CustomRegisterName?.StartsWith('.') ?? false)
        {
            Logger.Log($"Not registering {obj}: A CustomRegisterName may not starts with '.'");
            return;
        }

        if (obj.IsService)
            await currentConnection.RegisterServiceAsync(serviceName).ConfigureAwait(false);

        await resolveOwnerTask(obj).ConfigureAwait(false);

        OnObjectRegisteredToConnection?.Invoke(obj);
    }

    /// <summary>
    /// 从当前连接移除DBus对象
    /// </summary>
    /// <param name="mdBusObject">要移除的对象</param>
    /// <param name="failSoft">对象不在注册表中时返回<see cref="RegisterResult.FAILED"/>而不是抛出异常</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">给定的对象不在注册表中</exception>
    public RegisterResult UnRegisterObject(IMDBusObject? mdBusObject, bool failSoft = false)
    {
        Logger.Log($"Unregister object: {mdBusObject}", level: LogLevel.Debug);

        if (mdBusObject == null)
            return RegisterResult.NULL_OBJECT;

        if (!registedObjects.ContainsKey(mdBusObject))
            return RegisterResult.NO_SUCH_OBJECT;

        string? srvName = registedObjects.GetValueOrDefault(mdBusObject);

        if (!registedObjects.TryRemove(mdBusObject, out _))
        {
            if (failSoft) return RegisterResult.FAILED;

            throw new InvalidOperationException($"The given object ({mdBusObject}) does not exist in the current registry");
        }

        this.currentConnection?.UnregisterObject(mdBusObject);

        Task.Run(() => unRegisterFromConnectionTask(mdBusObject, srvName!));
        return RegisterResult.OK;
    }

    /// <summary>
    /// 将目标对象从DBus连接上移除
    /// <br/>
    /// 如果连接没准备好，则不会做任何事
    /// </summary>
    private async Task unRegisterFromConnectionTask(IMDBusObject mdBusObject, string serviceName)
    {
        if (!ConnectionReady()) return;

        this.currentConnection!.UnregisterObject(mdBusObject);

        if (mdBusObject.IsService)
            await this.currentConnection!.UnregisterServiceAsync(serviceName).ConfigureAwait(false);
    }

    #endregion

    #region Object Resolving

    private readonly List<string> resolvedServiceNames = new List<string>();

    private async Task resolveOwnerTask(IMDBusObject obj)
    {
        if (!ConnectionReady())
            return;

        string serviceName = registedObjects[obj];

        Debug.Assert(currentConnection != null, nameof(currentConnection) + " != null");

        if (!resolvedServiceNames.Contains(serviceName))
        {
            await currentConnection.ResolveServiceOwnerAsync
            (
                serviceName,
                onServiceNameChanged,
                e => onServiceError(e, serviceName)
            ).ConfigureAwait(false);

            resolvedServiceNames.Add(serviceName);
        }

        Logger.Log($"为{obj.ObjectPath}注册{serviceName}", level: LogLevel.Debug);
    }

    private void onServiceNameChanged(ServiceOwnerChangedEventArgs args)
    {
        Logger.Log($"服务 '{args.ServiceName}' 的归属现在从 '{args.OldOwner}' 变为 '{args.NewOwner}'");
    }

    private void onServiceError(Exception e, string serviceName)
    {
        if (e is ObjectDisposedException) return;

        Logger.Error(e, $"位于 '{serviceName}' 的DBus服务出现错误");
    }

    #endregion

    public Action? OnConnected;
    public Action<IDBusObject>? OnObjectRegisteredToConnection;

    /// <summary>
    /// Gets a proxy object to the specific dbus path or service name.
    /// </summary>
    /// <exception cref="NotSupportedException">Connection is not ready</exception>
    public S GetProxyObject<S>(ObjectPath path, string? name)
        where S : IDBusObject
    {
        if (!ConnectionReady())
            throw new NotSupportedException("未连接");

        if (string.IsNullOrEmpty(name))
            name = path.ToServiceName();

        Debug.Assert(currentConnection != null, nameof(currentConnection) + " != null");
        return currentConnection.CreateProxy<S>(name, path);
    }

    public bool ConnectionReady()
    {
        return this.ConnectionState == ConnectionState.Connected && this.currentConnection != null;
    }

    protected override void Dispose(bool isDisposing)
    {
        this.Disconnect();

        base.Dispose(isDisposing);
    }

    public enum RegisterResult
    {
        OK,
        FAILED,
        PATH_ALREADY_IN_USE,
        NULL_OBJECT,
        NO_SUCH_OBJECT
    }
}
