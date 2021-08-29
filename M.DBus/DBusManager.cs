using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Logging;
using Tmds.DBus;

namespace M.DBus
{
    public class DBusManager : IDisposable
    {
        private Connection currentConnection;
        private readonly List<IDBusObject> dBusObjects = new List<IDBusObject>();

        private ConnectionState connectionState = ConnectionState.NotConnected;

        public DBusManager(bool startOnLoad)
        {
            //如果在初始化时启动服务
            if (startOnLoad)
                Connect();
        }

        public string ObjectPathToName(ObjectPath path)
        {
            return path.ToString().Replace('/', '.').Remove(0, 1);
        }

        public static string ObjectPathToNameStatic(ObjectPath path)
        {
            return path.ToString().Replace('/', '.').Remove(0, 1);
        }

        //bug: 注册的服务/物件在dbus-send后会直接Name Lost，无法恢复
        private async Task registerObjects()
        {
            Logger.Log("注册DBus物件及服务...");

            //注册所有DBus Object
            await currentConnection.RegisterObjectsAsync(dBusObjects.ToArray()).ConfigureAwait(false);

            //递归注册DBus服务
            foreach (var dBusObject in dBusObjects)
            {
                await currentConnection.RegisterServiceAsync(ObjectPathToName(dBusObject.ObjectPath)).ConfigureAwait(false);
                await currentConnection.ResolveServiceOwnerAsync(
                    ObjectPathToName(dBusObject.ObjectPath),
                    onServiceNameChanged,
                    e => onServiceError(e, dBusObject)).ConfigureAwait(false);
            }
        }

        private void onServiceNameChanged(ServiceOwnerChangedEventArgs args)
        {
            Logger.Log($"服务 {args.ServiceName} 的归属现在从 {args.OldOwner} 变为 {args.NewOwner}");
        }

        private void onServiceError(Exception e, IDBusObject dbusObject)
        {
            connectionState = ConnectionState.Faulted;

            Logger.Error(e, $"位于 {ObjectPathToName(dbusObject.ObjectPath)} 的DBus服务出现错误");
        }

        public async Task RegisterNewObject(IDBusObject dbusObject)
        {
            //添加物件到列表
            dBusObjects.Add(dbusObject);

            if (connectionState == ConnectionState.Connected)
            {
                //注册物件
                await currentConnection.RegisterObjectAsync(dbusObject).ConfigureAwait(false);

                //注册服务
                await currentConnection.RegisterServiceAsync(ObjectPathToName(dbusObject.ObjectPath)).ConfigureAwait(false);
            }
        }

        public async Task RegisterNewObjects(IDBusObject[] objects)
        {
            //添加物件到列表
            dBusObjects.AddRange(objects);

            if (connectionState == ConnectionState.Connected)
            {
                //注册物件和DBus服务
                foreach (var dBusObject in dBusObjects)
                {
                    await currentConnection.RegisterObjectAsync(dBusObject).ConfigureAwait(false);

                    await currentConnection.RegisterServiceAsync(ObjectPathToName(dBusObject.ObjectPath)).ConfigureAwait(false);
                }
            }
        }

        public void UnRegisterObject(IDBusObject dBusObject)
        {
            var target = dBusObjects.FirstOrDefault(o => o.ObjectPath.Equals(dBusObject.ObjectPath));

            if (target != null)
            {
                Logger.Log($"反注册{dBusObject.ObjectPath}");
                dBusObjects.Remove(target);
                currentConnection.UnregisterObject(target);
                Task.Run(() => unregisterObjectTask(target));
            }
            else
            {
                throw new InvalidOperationException("尝试反注册一个不存在的DBus物件");
            }
        }

        private async Task unregisterObjectTask(IDBusObject target)
        {
            await currentConnection.UnregisterServiceAsync(ObjectPathToName(target.ObjectPath)).ConfigureAwait(false);
        }

        public bool CheckIfAlreadyRegistered(IDBusObject dBusObject)
            => dBusObjects.Any(o => o.ObjectPath.Equals(dBusObject.ObjectPath));

        public bool CheckIfAlreadyRegistered(ObjectPath objectPath)
            => dBusObjects.Any(o => o.ObjectPath.Equals(objectPath));

        private CancellationTokenSource cancellationTokenSource;

        public void Connect()
        {
            if (isDisposed)
                throw new ObjectDisposedException(ToString(), "已处理的对象不能再次连接。");

            //先停止服务
            Disconnect();

            Logger.Log("正在启动 DBUS 服务!");

            //刷新cancellationToken
            cancellationTokenSource = new CancellationTokenSource();

            //开始服务
            Task.Run(connectTask, cancellationTokenSource.Token);
        }

        public bool Disconnect()
        {
            Logger.Log("正在停止 DBUS 服务!");

            try
            {
                //如果已经连接到DBus
                if (connectionState == ConnectionState.Connected)
                {
                    //反注册物件
                    currentConnection.UnregisterObjects(dBusObjects);

                    //反注册服务
                    foreach (var dBusObject in dBusObjects)
                    {
                        currentConnection.UnregisterServiceAsync(ObjectPathToName(dBusObject.ObjectPath)).ConfigureAwait(false);
                    }
                }
                else if (connectionState == ConnectionState.Faulted)
                {
                    currentConnection.Dispose();
                    currentConnection = null;
                    connectionState = ConnectionState.NotConnected;
                    Logger.Log("DBus服务已经出现过一次错误, 将处理此次连接。");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "停止DBus服务时出现了错误");

                return false;
            }

            return true;
        }

        private async Task connectTask()
        {
            switch (connectionState)
            {
                case ConnectionState.NotConnected:
                    //初始化到DBus的连接
                    currentConnection ??= new Connection(Address.Session);

                    //连接到DBus
                    connectionState = ConnectionState.Connecting;

                    //启动服务
                    await connectAsync().ConfigureAwait(false);
                    await registerObjects().ConfigureAwait(false);
                    break;

                case ConnectionState.Connected:
                    Logger.Log("连接过了，直接注册!");

                    //直接注册
                    await registerObjects().ConfigureAwait(false);
                    break;
            }
        }

        private async Task connectAsync()
        {
            //等待连接
            await currentConnection.ConnectAsync().ConfigureAwait(false);

            //设置连接状态
            connectionState = ConnectionState.Connected;
        }

        private enum ConnectionState
        {
            NotConnected,
            Connecting,
            Connected,
            Faulted
        }

        public async Task GetAllServices()
        {
            await currentConnection.ListServicesAsync().ConfigureAwait(false);
            var services = await currentConnection.ListServicesAsync().ConfigureAwait(false);

            foreach (var service in services)
            {
                Logger.Log(service);
            }
        }

        private bool isDisposed { get; set; }

        public void Dispose()
        {
            Disconnect();
            currentConnection?.Dispose();
            cancellationTokenSource?.Dispose();

            isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
