using System.Threading.Tasks;
using Tmds.DBus;

namespace M.DBus.Services
{
    [DBusInterface("io.matrix_feather.dbus.greet")]
    public interface IGreet : IDBusObject
    {
        Task<string> GreetAsync(string message);
    }

    public class Greet : IGreet
    {
        public static readonly ObjectPath PATH = new ObjectPath("/io/matrix_feather/dbus/greet");
        private string host;

        public Greet(string host = "null")
        {
            this.host = host;
        }

        public Task<string> GreetAsync(string name)
        {
            return Task.FromResult($"Greetings from {GetType().Assembly} : {host} , {name}!");
        }

        public ObjectPath ObjectPath => PATH;
    }
}
