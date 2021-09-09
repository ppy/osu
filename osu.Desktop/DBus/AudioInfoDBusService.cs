using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Tmds.DBus;

namespace osu.Desktop.DBus
{
    [DBusInterface("io.matrix_feather.mfosu.Audio")]
    public interface IAudioInfoDBusService : IDBusObject
    {
        Task<AudioInfoProperties> GetAllAsync();
        Task<object> GetAsync(string prop);
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
        Task<double> GetTrackLengthAsync();
        Task<double> GetTrackProgressAsync();
    }

    [Dictionary]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    public class AudioInfoProperties
    {
        private double _Length;

        public double Length
        {
            get => _Length;
            set
            {
                _Length = value;
                dictionary[nameof(Length)] = value;
            }
        }

        private double _Current;

        public double Current
        {
            get => _Current;
            set
            {
                _Current = value;
                dictionary[nameof(Current)] = value;
            }
        }

        internal bool SetValue(string name, object rawValue)
        {
            var value = (double)rawValue;

            switch (name)
            {
                case nameof(Length):
                    if (Length != value)
                    {
                        Length = value;
                        return true;
                    }

                    return false;

                case nameof(Current):
                    if (Current != value)
                    {
                        Current = value;
                        return true;
                    }

                    return false;
            }

            return false;
        }

        private readonly IDictionary<string, object> dictionary = new ConcurrentDictionary<string, object>
        {
            [nameof(Length)] = 0d,
            [nameof(Current)] = 0d,
        };

        internal IDictionary<string, object> ToDictionary() => dictionary;
    }

    public class AudioInfoDBusService : IAudioInfoDBusService
    {
        public ObjectPath ObjectPath => PATH;
        public static readonly ObjectPath PATH = new ObjectPath("/io/matrix_feather/mfosu/Audio");

        private readonly AudioInfoProperties properties = new AudioInfoProperties();

        public double Length
        {
            set => setProperty(nameof(AudioInfoProperties.Length), value);
        }

        public double Current
        {
            set => setProperty(nameof(AudioInfoProperties.Current), value);
        }

        private void setProperty(string target, object value)
        {
            if (properties.SetValue(target, value))
                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty(target, value));
        }

        public Task<AudioInfoProperties> GetAllAsync() => Task.FromResult(properties);

        public Task<object> GetAsync(string prop)
            => Task.FromResult(properties.ToDictionary()[prop]);

        public Task SetAsync(string prop, object val)
        {
            throw new InvalidOperationException("只读属性");
        }

        public event Action<PropertyChanges> OnPropertiesChanged;

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
            => SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);

        public Task<double> GetTrackLengthAsync()
            => Task.FromResult(properties.Length);

        public Task<double> GetTrackProgressAsync()
            => Task.FromResult(properties.Current);
    }
}
