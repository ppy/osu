using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using Tmds.DBus;

namespace M.DBus
{
    public class DBusManager : Component
    {
        private Connection currentConnection;
        private readonly List<IDBusObject> dBusObjects = new List<IDBusObject>();

        private ConnectionState connectionState = ConnectionState.NotConnected;

        private readonly Bindable<bool> controlSource;

        public DBusManager(bool startOnLoad, bool autoStart = false, Bindable<bool> controlSource = null)
        {
            //如果在初始化时启动服务
            if (startOnLoad)
                Connect();

            if (autoStart && controlSource != null)
            {
                this.controlSource = controlSource;
            }
            else if (controlSource == null && autoStart)
            {
                throw new InvalidOperationException("设置了自动启动但是控制源是null?");
            }
        }

        protected override void LoadComplete()
        {
            controlSource.BindValueChanged(onControlSourceChanged, true);
            base.LoadComplete();
        }

        private void onControlSourceChanged(ValueChangedEvent<bool> v)
        {
            if (v.NewValue)
                Connect();
            else
                Disconnect();
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
            Logger.Log("注册Obj");

            //注册所有DBus Object
            await currentConnection.RegisterObjectsAsync(dBusObjects.ToArray()).ConfigureAwait(false);

            //递归注册DBus服务
            foreach (var dBusObject in dBusObjects)
            {
                await currentConnection.RegisterServiceAsync(ObjectPathToName(dBusObject.ObjectPath)).ConfigureAwait(false);
                await currentConnection.ResolveServiceOwnerAsync(
                    ObjectPathToName(dBusObject.ObjectPath),
                    onServiceNameChanged,
                    e => onServiceError(e, ObjectPathToName(dBusObject.ObjectPath))).ConfigureAwait(false);
            }
        }

        private void onServiceNameChanged(ServiceOwnerChangedEventArgs args)
        {
            Logger.Log($"服务 {args.ServiceName} 的归属现在从 {args.OldOwner} 变为 {args.NewOwner}");
        }

        private void onServiceError(Exception e, string path)
        {
            connectionState = ConnectionState.Faulted;

            Logger.Error(e, $"位于 {path} 的DBus服务出现错误, 请尝试重新连接到DBus");
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
                //注册物件
                await currentConnection.RegisterObjectsAsync(objects).ConfigureAwait(false);

                //注册DBus服务
                foreach (var dBusObject in dBusObjects)
                {
                    await currentConnection.RegisterServiceAsync(ObjectPathToName(dBusObject.ObjectPath)).ConfigureAwait(false);
                }
            }
        }

        private CancellationTokenSource cancellationTokenSource;

        public void Connect()
        {
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
            }
            catch (Exception e)
            {
                if (connectionState == ConnectionState.NotConnected)
                {
                    currentConnection.Dispose();
                    currentConnection = null;
                    Logger.Log("DBus服务已经出现过一次错误, 将处理此次连接。");
                }

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

        protected override void Dispose(bool isDisposing)
        {
            Disconnect();
            base.Dispose(isDisposing);
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
    }
}
