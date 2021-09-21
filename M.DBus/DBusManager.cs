using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using M.DBus.Services;
using M.DBus.Tray;
using M.DBus.Utils;
using osu.Framework.Logging;
using Tmds.DBus;

namespace M.DBus
{
    public class DBusManager : IDisposable
    {
        public Action OnConnected;

        public Greet GreetService = new Greet();
        private Connection currentConnection;

        private ConnectionState connectionState = ConnectionState.NotConnected;

        private bool isDisposed { get; set; }

        public readonly IHandleTrayManagement TrayManager;

        public readonly IHandleSystemNotifications Notifications;

        public DBusManager(bool startOnLoad, IHandleTrayManagement trayManagement, IHandleSystemNotifications systemNotifications)
        {
            //如果在初始化时启动服务
            if (startOnLoad)
                Connect();

            TrayManager = trayManagement;
            Notifications = systemNotifications;

            Task.Run(() => RegisterNewObject(GreetService));
        }

        #region Disposal

        public void Dispose()
        {
            Disconnect();

            isDisposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion

        #region 断开连接

        public bool Disconnect()
        {
            Logger.Log("正在断开连接!");

            try
            {
                switch (connectionState)
                {
                    case ConnectionState.Connected:
                        GreetService.SwitchState(false, "Diconnecting");

                        //反注册物件
                        currentConnection.UnregisterObjects(registerDictionary.Keys);

                        //反注册服务
                        foreach (var dBusObject in registerDictionary.Keys)
                            unRegisterFromConnection(dBusObject).ConfigureAwait(false);

                        //清除当前连接目标
                        currentConnectTarget = string.Empty;

                        break;

                    case ConnectionState.Connecting:
                        //如果正在连接，中断当前任务
                        cancellationTokenSource?.Cancel();
                        break;

                    case ConnectionState.Faulted:
                        currentConnection.Dispose();
                        currentConnection = null;
                        connectionState = ConnectionState.NotConnected;
                        Logger.Log("DBus服务已经出现过一次错误, 将处理此次连接。");
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "停止DBus服务时出现了错误");

                return false;
            }

            return true;
        }

        #endregion

        #region 工具

        public async Task GetAllServices()
        {
            await currentConnection.ListServicesAsync().ConfigureAwait(false);
            var services = await currentConnection.ListServicesAsync().ConfigureAwait(false);

            foreach (var service in services) Logger.Log(service);
        }

        private void onServiceNameChanged(ServiceOwnerChangedEventArgs args)
        {
            Logger.Log($"服务 '{args.ServiceName}' 的归属现在从 '{args.OldOwner}' 变为 '{args.NewOwner}'");
        }

        private void onServiceError(Exception e, IDBusObject dbusObject)
        {
            connectionState = ConnectionState.Faulted;

            Logger.Error(e, $"位于 '{dbusObject.ObjectPath.ToServiceName()}' 的DBus服务出现错误");
        }

        public bool CheckIfAlreadyRegistered(IDBusObject dBusObject)
        {
            return registerDictionary.Any(o => o.Key.ObjectPath.Equals(dBusObject.ObjectPath));
        }

        public bool CheckIfAlreadyRegistered(ObjectPath objectPath)
        {
            return registerDictionary.Any(o => o.Key.ObjectPath.Equals(objectPath));
        }

        public T GetDBusObject<T>(ObjectPath path, string name = null)
            where T : IDBusObject
        {
            if (connectionState != ConnectionState.Connected || currentConnection == null)
                throw new NotSupportedException("未连接");

            if (string.IsNullOrEmpty(name))
                name = path.ToServiceName();

            return currentConnection.CreateProxy<T>(name, path);
        }

        #endregion

        #region 注册新对象

        private readonly Dictionary<IDBusObject, string> registerDictionary = new Dictionary<IDBusObject, string>();

        public async Task RegisterNewObject(IDBusObject dbusObject, string targetName = null)
        {
            if (string.IsNullOrEmpty(targetName))
                targetName = dbusObject.ObjectPath.ToServiceName();

            //添加物件与其目标名称添加到词典
            registerDictionary[dbusObject] = targetName;

            if (connectionState == ConnectionState.Connected)
                await registerToConection(dbusObject).ConfigureAwait(false);
        }

        public async Task RegisterNewObjects(IDBusObject[] objects)
        {
            //添加物件到列表
            foreach (var obj in objects)
                registerDictionary[obj] = obj.ObjectPath.ToServiceName();

            if (connectionState == ConnectionState.Connected)
            {
                //注册物件和DBus服务
                foreach (var dBusObject in registerDictionary.Keys)
                    await registerToConection(dBusObject).ConfigureAwait(false);
            }
        }

        //bug: 注册的服务/物件在错误的dbus-send后会直接Name Lost，无法恢复
        private async Task registerObjects()
        {
            Logger.Log("注册DBus物件及服务...");

            //递归注册DBus服务
            foreach (var dBusObject in registerDictionary.Keys)
                await registerToConection(dBusObject).ConfigureAwait(false);
        }

        private readonly List<string> registeredServices = new List<string>();

        private async Task registerToConection(IDBusObject dBusObject)
        {
            var targetName = registerDictionary[dBusObject];
            await currentConnection.RegisterObjectAsync(dBusObject).ConfigureAwait(false);

            await currentConnection.RegisterServiceAsync(targetName).ConfigureAwait(false);

            if (!registeredServices.Contains(targetName))
            {
                await currentConnection.ResolveServiceOwnerAsync(
                    targetName,
                    onServiceNameChanged,
                    e => onServiceError(e, dBusObject)).ConfigureAwait(false);

                registeredServices.Add(targetName);
            }

            Logger.Log($"为{dBusObject.ObjectPath}注册{targetName}");
        }

        #endregion

        #region 反注册对象

        public void UnRegisterObject(IDBusObject dBusObject)
        {
            var target = registerDictionary.FirstOrDefault(o => o.Key.ObjectPath.Equals(dBusObject.ObjectPath)).Key;

            if (target != null)
            {
                Logger.Log($"反注册{dBusObject.ObjectPath}");
                Task.Run(() =>
                {
                    unRegisterFromConnection(target).ConfigureAwait(false);
                    registerDictionary.Remove(target);
                });
            }
            else
                throw new InvalidOperationException("尝试反注册一个不存在的DBus物件");
        }

        private async Task unRegisterFromConnection(IDBusObject dBusObject)
        {
            currentConnection.UnregisterObject(dBusObject);
            await currentConnection.UnregisterServiceAsync(registerDictionary[dBusObject]).ConfigureAwait(false);
        }

        #endregion

        #region 连接到DBus

        private CancellationTokenSource cancellationTokenSource;

        private string currentConnectTarget;

        public void Connect(string target = null)
        {
            if (isDisposed)
                throw new ObjectDisposedException(ToString(), "已处理的对象不能再次连接。");

            //默认连接到会话
            if (string.IsNullOrEmpty(Address.Session))
                throw new AddressNotFoundException("会话地址为空，请检查dbus服务是否已经启动");

            target ??= Address.Session;
            currentConnectTarget = target;

            //先停止服务
            Disconnect();

            Logger.Log($"正在连接到 {target} 上的DBus服务!");

            //刷新cancellationToken
            cancellationTokenSource = new CancellationTokenSource();

            //开始服务
            Task.Run(() => connectTask(target), cancellationTokenSource.Token);
        }

        private async Task connectTask(string target)
        {
            try
            {
                switch (connectionState)
                {
                    case ConnectionState.NotConnected:
                        //初始化到DBus的连接
                        currentConnection ??= new Connection(target);

                        //连接到DBus
                        connectionState = ConnectionState.Connecting;

                        //等待连接
                        await currentConnection.ConnectAsync().ConfigureAwait(false);

                        //设置连接状态
                        connectionState = ConnectionState.Connected;

                        await registerObjects().ConfigureAwait(false);
                        OnConnected?.Invoke();
                        GreetService.SwitchState(true, "");
                        break;

                    case ConnectionState.Connected:
                        Logger.Log($"已经连接到{currentConnectTarget}，直接注册!");

                        //直接注册
                        await registerObjects().ConfigureAwait(false);
                        OnConnected?.Invoke();
                        GreetService.SwitchState(true, "");
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "连接到DBus时出现错误");
            }
        }

        #endregion

        private enum ConnectionState
        {
            NotConnected,
            Connecting,
            Connected,
            Faulted
        }

        private class AddressNotFoundException : InvalidOperationException
        {
            public AddressNotFoundException(string s)
                : base(s)
            {
            }
        }
    }
}
