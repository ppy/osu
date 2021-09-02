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
            return Task.FromResult($"Greetings from {GetType().Assembly} : {host} , {name}!");
        }

        public Task<bool> SendMessageAsync(string message)
        {
            if (AllowPost.Value && !string.IsNullOrEmpty(message))
                OnMessageRecive?.Invoke(message);

            return Task.FromResult(AllowPost.Value);
        }

        public ObjectPath ObjectPath => PATH;
    }
}
