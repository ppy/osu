using System;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using Tmds.DBus;

namespace M.DBus.Services
{
    [DBusInterface("io.matrix_feather.dbus.greet")]
    public interface IGreet : IDBusObject
    {
        Task<string> GreetAsync(string message);
        Task<bool> SendMessageAsync(string message);
        Task<bool> GetOnlineStateAsync();
        public Task<IDisposable> WatchOfflineAsync(Action<string> handler);
        public Task<IDisposable> WatchOnlineAsync(Action<string> handler);
    }

    public class Greet : IGreet
    {
        public static readonly ObjectPath PATH = new ObjectPath("/io/matrix_feather/dbus/greet");
        private readonly string host;

        public Action<string> OnMessageRecive;
        public Bindable<bool> AllowPost;

        public Greet(string host = "null")
        {
            this.host = host;
        }

        public Task<string> GreetAsync(string name)
        {
            return Task.FromResult($"{host}");
        }

        public Task<bool> SendMessageAsync(string message)
        {
            if (AllowPost.Value && !string.IsNullOrEmpty(message))
                OnMessageRecive?.Invoke(message);

            return Task.FromResult(AllowPost.Value);
        }

        public Task<bool> GetOnlineStateAsync() => Task.FromResult(isOnline);

        private bool isOnline;

        public void SwitchState(bool online, string reason)
        {
            if (online)
                GoingOnline?.Invoke(reason);
            else
                GoingOffline?.Invoke(reason);

            this.isOnline = online;
        }

        public event Action<string> GoingOffline;

        public Task<IDisposable> WatchOfflineAsync(Action<string> handler)
            => SignalWatcher.AddAsync(this, nameof(GoingOffline), handler);

        public event Action<string> GoingOnline;

        public Task<IDisposable> WatchOnlineAsync(Action<string> handler)
            => SignalWatcher.AddAsync(this, nameof(GoingOnline), handler);

        public ObjectPath ObjectPath => PATH;
    }
}
