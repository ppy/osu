using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Mvis.Plugin.CloudMusicSupport.DBus
{
    [DBusInterface("io.matrix_feather.mvis.lyric")]
    public interface ILyricDBusObject : IDBusObject
    {
        Task<IDictionary<string, object>> GetAllAsync();
        Task<object> GetAsync(string prop);
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
        Task<string> GetCurrentLineRawAsync();
        Task<string> GetCurrentLineTranslatedAsync();
    }

    public class LyricDBusObject : ILyricDBusObject
    {
        public static readonly ObjectPath PATH = new ObjectPath("/io/matrix_feather/mvis/lyric");
        public ObjectPath ObjectPath => PATH;

        internal string RawLyric
        {
            set
            {
                value ??= string.Empty;
                properties["raw"] = value;
                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty("raw", value));
            }
        }

        internal string TranslatedLyric
        {
            set
            {
                value ??= string.Empty;
                properties["translate"] = value;
                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty("translate", value));
            }
        }

        private readonly IDictionary<string, object> properties = new ConcurrentDictionary<string, object>
        {
            ["raw"] = "",
            ["translate"] = ""
        };

        public Task<IDictionary<string, object>> GetAllAsync()
            => Task.FromResult(properties);

        public Task<object> GetAsync(string prop)
            => Task.FromResult(properties[prop]);

        public Task SetAsync(string prop, object val)
        {
            throw new NotImplementedException();
        }

        public event Action<PropertyChanges> OnPropertiesChanged;

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
            => SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);

        public Task<string> GetCurrentLineRawAsync()
            => Task.FromResult(properties["raw"] as string);

        public Task<string> GetCurrentLineTranslatedAsync()
            => Task.FromResult(properties["translate"] as string);
    }
}
