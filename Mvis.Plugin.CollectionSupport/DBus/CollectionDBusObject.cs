using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Mvis.Plugin.CollectionSupport.DBus
{
    [DBusInterface("io.matrix_feather.mvis.collection")]
    public interface ICollectionDBusObject : IDBusObject
    {
        Task<IDictionary<string, object>> GetAllAsync();
        Task<object> GetAsync(string prop);
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
        Task<string> GetCurrentCollectionNameAsync();
        Task<int> GetCurrentIndexAsync();
    }

    public class CollectionDBusObject : ICollectionDBusObject
    {
        public static readonly ObjectPath PATH = new ObjectPath("/io/matrix_feather/mvis/collection");
        public ObjectPath ObjectPath => PATH;

        internal string CollectionName
        {
            set
            {
                value ??= string.Empty;
                properties["name"] = value;
                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty("name", value));
            }
        }

        internal int Position
        {
            set
            {
                properties["position"] = value;
                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty("position", value));
            }
        }

        private readonly IDictionary<string, object> properties = new ConcurrentDictionary<string, object>
        {
            ["name"] = "",
            ["position"] = -1
        };

        public Task<IDictionary<string, object>> GetAllAsync()
            => Task.FromResult(properties);

        public Task<object> GetAsync(string prop)
            => Task.FromResult(properties[prop]);

        public Task SetAsync(string prop, object val)
        {
            throw new System.NotImplementedException();
        }

        public event Action<PropertyChanges> OnPropertiesChanged;

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
            => SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);

        public Task<string> GetCurrentCollectionNameAsync()
            => Task.FromResult(properties["name"] as string);

        public Task<int> GetCurrentIndexAsync()
            => Task.FromResult(properties["position"] as int? ?? -1);
    }
}
