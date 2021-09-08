using System.Threading.Tasks;
using Tmds.DBus;

namespace Mvis.Plugin.Example.DBus
{
    [DBusInterface("io.matrix_feather.mvis.example")]
    public interface IExampleDBusObject : IDBusObject
    {
        Task<string> ExampleFunctionAsync();
    }

    public class ExampleDBusObject : IExampleDBusObject
    {
        public ObjectPath ObjectPath => new ObjectPath("/io/matrix_feather/mvis/example");

        public Task<string> ExampleFunctionAsync()
        {
            return Task.FromResult("Hello World!");
        }
    }
}
