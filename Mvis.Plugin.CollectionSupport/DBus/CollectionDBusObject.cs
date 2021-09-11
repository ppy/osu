using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using M.DBus;
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
        public string Name
        {
            get => _Name;
            set => _Name = value;
        }

        public int Position
        {
            get => _Position;
            set => _Position = value;
        }

        private string _Name = string.Empty;

        private int _Position;

        private IDictionary<string, object> members;

        public object Get(string prop)
        {
            ServiceUtils.CheckIfDirectoryNotReady(this, members, out members);
            return ServiceUtils.GetValueFor(this, prop, members);
        }

        internal bool Set(string name, object newValue)
        {
            ServiceUtils.CheckIfDirectoryNotReady(this, members, out members);
            return ServiceUtils.SetValueFor(this, name, newValue, members);
        }
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
                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty(nameof(CollectionProperties.Name), value));
            }
        }

        internal int Position
        {
            set
            {
                properties.Position = value;
                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty(nameof(CollectionProperties.Position), value));
            }
        }

        private readonly CollectionProperties properties = new CollectionProperties();

        public Task<CollectionProperties> GetAllAsync()
        {
            return Task.FromResult(properties);
        }

        public Task<object> GetAsync(string prop)
        {
            return Task.FromResult(properties.Get(prop));
        }

        public Task SetAsync(string prop, object val)
        {
            throw new InvalidOperationException("只读属性");
        }

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
        {
            return SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);
        }

        public Task<string> GetCurrentCollectionNameAsync()
        {
            return Task.FromResult(properties.Name);
        }

        public Task<int> GetCurrentIndexAsync()
        {
            return Task.FromResult(properties.Position);
        }

        public event Action<PropertyChanges> OnPropertiesChanged;
    }
}
