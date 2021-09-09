using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using osu.Game.Users;
using Tmds.DBus;

namespace osu.Desktop.DBus
{
    [DBusInterface("io.matrix_feather.mfosu.CurrentUser")]
    public interface IUserInfoDBusService : IDBusObject
    {
        Task<UserMetadataProperties> GetAllAsync();
        Task<object> GetAsync(string prop);
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);

        Task<string> GetNameAsync();
        Task<string> GetActivityAsync();
        Task<int> GetGlobalRankAsync();
        Task<int> GetRegionRankAsync();
        Task<string> GetAvatarUrlAsync();
        Task<int> GetPPAsync();
        Task<string> GetCurrentRulesetAsync();
        Task<long> GetLaunchTickAsync();
    }

    [Dictionary]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    public class UserMetadataProperties
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

        private int _GlobalRank;

        public int GlobalRank
        {
            get => _GlobalRank;
            set
            {
                _GlobalRank = value;
                dictionary[nameof(GlobalRank)] = value;
            }
        }

        private int _RegionRank;

        public int RegionRank
        {
            get => _RegionRank;
            set
            {
                _RegionRank = value;
                dictionary[nameof(RegionRank)] = value;
            }
        }

        private string _AvatarUrl;

        public string AvatarUrl
        {
            get => _AvatarUrl;
            set
            {
                _AvatarUrl = value;
                dictionary[nameof(AvatarUrl)] = value;
            }
        }

        private int _PP;

        public int PP
        {
            get => _PP;
            set
            {
                _PP = value;
                dictionary[nameof(PP)] = value;
            }
        }

        private string _CurrentRuleset;

        public string CurrentRuleset
        {
            get => _CurrentRuleset;
            set
            {
                _CurrentRuleset = value;
                dictionary[nameof(CurrentRuleset)] = value;
            }
        }

        private string _Activity;

        public string Activity
        {
            get => _Activity;
            set
            {
                _Activity = value;
                dictionary[nameof(Activity)] = value;
            }
        }

        private readonly IDictionary<string, object> dictionary = new ConcurrentDictionary<string, object>
        {
            [nameof(Name)] = string.Empty,
            [nameof(GlobalRank)] = 0,
            [nameof(RegionRank)] = 0,
            [nameof(AvatarUrl)] = string.Empty,
            [nameof(PP)] = (decimal)0,
            [nameof(CurrentRuleset)] = string.Empty,
            [nameof(Activity)] = string.Empty
        };

        internal IDictionary<string, object> ToDictionary() => dictionary;

        internal bool SetValue(string name, object value)
        {
            switch (name)
            {
                case nameof(Name):
                    Name = (string)value;
                    return true;

                case nameof(GlobalRank):
                    GlobalRank = (int)value;
                    return true;

                case nameof(RegionRank):
                    RegionRank = (int)value;
                    return true;

                case nameof(AvatarUrl):
                    AvatarUrl = (string)value;
                    return true;

                case nameof(PP):
                    PP = (int)value;
                    return true;

                case nameof(CurrentRuleset):
                    CurrentRuleset = (string)value;
                    return true;

                case nameof(Activity):
                    Activity = (string)value;
                    return true;
            }

            return false;
        }
    }

    public class UserInfoDBusService : IUserInfoDBusService
    {
        public ObjectPath ObjectPath => PATH;
        public static readonly ObjectPath PATH = new ObjectPath("/io/matrix_feather/mfosu/CurrentUser");

        private readonly TimeSpan loadTick = new TimeSpan(DateTime.Now.Ticks);

        public User User
        {
            set
            {
                SetProperty(nameof(properties.Name), value.Username);
                SetProperty(nameof(properties.GlobalRank), value.Statistics?.GlobalRank > 0 ? value.Statistics.GlobalRank : 0);
                SetProperty(nameof(properties.RegionRank), value.Statistics?.CountryRank > 0 ? value.Statistics.CountryRank : 0);
                SetProperty(nameof(properties.AvatarUrl), value.AvatarUrl ?? string.Empty);
                SetProperty(nameof(properties.PP), value.Statistics?.PP > 0 ? decimal.ToInt32((decimal)value.Statistics.PP) : 0);
            }
        }

        internal void SetProperty(string target, object value)
        {
            if (properties.SetValue(target, value))
                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty(target, value));
        }

        private readonly UserMetadataProperties properties = new UserMetadataProperties();

        public Task<UserMetadataProperties> GetAllAsync()
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

        public Task<string> GetNameAsync()
            => Task.FromResult(properties.Name);

        public Task<string> GetActivityAsync()
            => Task.FromResult(properties.Activity);

        public Task<int> GetGlobalRankAsync()
            => Task.FromResult(properties.GlobalRank);

        public Task<int> GetRegionRankAsync()
            => Task.FromResult(properties.RegionRank);

        public Task<string> GetAvatarUrlAsync()
            => Task.FromResult(properties.AvatarUrl);

        public Task<int> GetPPAsync()
            => Task.FromResult(properties.PP);

        public Task<string> GetCurrentRulesetAsync()
            => Task.FromResult(properties.CurrentRuleset);

        public Task<long> GetLaunchTickAsync()
            => Task.FromResult(new TimeSpan(DateTime.Now.Ticks).Ticks - loadTick.Ticks);
    }
}
