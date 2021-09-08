using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Mvis.Plugin.CollectionSupport.DBus
{
    [DBusInterface("io.matrix_feather.mvis.collection")]
    public interface ICollectionDBusObject : IDBusObject
    {
        Task<CollectionProperties> GetAllAsync();
        Task<object> GetAsync(string prop);
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
        Task<string> GetCurrentCollectionNameAsync();
        Task<int> GetCurrentIndexAsync();
    }

    [Dictionary]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    public class CollectionProperties
    {
        private string _Name = string.Empty;

        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                dictionary[nameof(Name)] = value;
            }
        }

        private int _Position;

        public int Position
        {
            get => _Position;
            set
            {
                _Position = value;
                dictionary[nameof(Position)] = value;
            }
        }

        private readonly IDictionary<string, object> dictionary = new ConcurrentDictionary<string, object>
        {
            [nameof(Name)] = string.Empty,
            [nameof(Position)] = 0
        };

        internal IDictionary<string, object> ToDictionary() => dictionary;
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
                properties.Name = value;
                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty("name", value));
            }
        }

        internal int Position
        {
            set
            {
                properties.Position = value;
                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty("position", value));
            }
        }

        private readonly CollectionProperties properties = new CollectionProperties();

        public Task<CollectionProperties> GetAllAsync()
            => Task.FromResult(properties);

        public Task<object> GetAsync(string prop)
            => Task.FromResult(properties.ToDictionary()[prop]);

        public Task SetAsync(string prop, object val)
        {
            throw new InvalidOperationException("只读属性");
        }

        public event Action<PropertyChanges> OnPropertiesChanged;

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
            => SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);

        public Task<string> GetCurrentCollectionNameAsync()
            => Task.FromResult(properties.Name);

        public Task<int> GetCurrentIndexAsync()
            => Task.FromResult(properties.Position);
    }
}
