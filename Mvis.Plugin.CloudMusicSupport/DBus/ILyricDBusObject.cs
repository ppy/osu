using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using M.DBus;
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
        public string RawString
        {
            get => _RawString;
            set => _RawString = value;
        }

        public string TranslatedString
        {
            get => _TranslatedString;
            set => _TranslatedString = value;
        }

        private string _RawString = string.Empty;

        private string _TranslatedString;

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
                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty(nameof(properties.RawString), value));
            }
        }

        internal string TranslatedLyric
        {
            set
            {
                value ??= string.Empty;
                properties.TranslatedString = value;
                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty(nameof(properties.TranslatedString), value));
            }
        }

        private readonly LyricProperties properties = new LyricProperties();

        public Task<LyricProperties> GetAllAsync()
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

        public Task<string> GetCurrentLineRawAsync()
        {
            return Task.FromResult(properties.RawString);
        }

        public Task<string> GetCurrentLineTranslatedAsync()
        {
            return Task.FromResult(properties.TranslatedString);
        }

        public event Action<PropertyChanges> OnPropertiesChanged;
    }
}
