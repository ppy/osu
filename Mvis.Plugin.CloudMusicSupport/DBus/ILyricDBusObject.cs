using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Mvis.Plugin.CloudMusicSupport.DBus
{
    [DBusInterface("io.matrix_feather.mvis.lyric")]
    public interface ILyricDBusObject : IDBusObject
    {
        Task<LyricProperties> GetAllAsync();
        Task<object> GetAsync(string prop);
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
        Task<string> GetCurrentLineRawAsync();
        Task<string> GetCurrentLineTranslatedAsync();
    }

    [Dictionary]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    public class LyricProperties
    {
        private string _RawString = string.Empty;

        public string RawString
        {
            get => _RawString;
            set
            {
                _RawString = value;
                dictionary[nameof(RawString)] = value;
            }
        }

        private string _TranslatedString;

        public string TranslatedString
        {
            get => _TranslatedString;
            set
            {
                _TranslatedString = value;
                dictionary[nameof(TranslatedString)] = value;
            }
        }

        private readonly IDictionary<string, object> dictionary = new ConcurrentDictionary<string, object>
        {
            [nameof(RawString)] = string.Empty,
            [nameof(TranslatedString)] = string.Empty
        };

        internal IDictionary<string, object> ToDictionary() => dictionary;
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
                properties.RawString = value;
                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty("raw", value));
            }
        }

        internal string TranslatedLyric
        {
            set
            {
                value ??= string.Empty;
                properties.TranslatedString = value;
                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty("translate", value));
            }
        }

        private readonly LyricProperties properties = new LyricProperties();

        public Task<LyricProperties> GetAllAsync()
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

        public Task<string> GetCurrentLineRawAsync()
            => Task.FromResult(properties.RawString);

        public Task<string> GetCurrentLineTranslatedAsync()
            => Task.FromResult(properties.TranslatedString);
    }
}
