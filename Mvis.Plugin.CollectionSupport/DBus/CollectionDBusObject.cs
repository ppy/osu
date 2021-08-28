using System.Threading.Tasks;
using Tmds.DBus;

namespace Mvis.Plugin.CollectionSupport.DBus
{
    [DBusInterface("io.matrix_feather.mvis.collection")]
    public interface ICollectionDBusObject : IDBusObject
    {
        Task<string> GetCurrentCollectionNameAsync();
        Task<int> GetCurrentIndexAsync();
    }

    public class CollectionDBusObject : ICollectionDBusObject
    {
        public static readonly ObjectPath PATH = new ObjectPath("/io/matrix_feather/mvis/collection");
        public ObjectPath ObjectPath => PATH;

        public CollectionHelper Plugin { get; set; }

        public Task<string> GetCurrentCollectionNameAsync()
            => Task.FromResult(Plugin.CurrentCollection.Value?.Name.Value ?? string.Empty);

        public Task<int> GetCurrentIndexAsync()
            => Task.FromResult(Plugin.CurrentPosition);
    }
}
