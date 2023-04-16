using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using M.DBus;
using osu.Game.Online.API.Requests.Responses;
using Tmds.DBus;

#nullable disable

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
        public string Name
        {
            get => _Name;
            set => _Name = value;
        }

        public int GlobalRank
        {
            get => _GlobalRank;
            set => _GlobalRank = value;
        }

        public int RegionRank
        {
            get => _RegionRank;
            set => _RegionRank = value;
        }

        public string AvatarUrl
        {
            get => _AvatarUrl;
            set => _AvatarUrl = value;
        }

        public int PP
        {
            get => _PP;
            set => _PP = value;
        }

        public string CurrentRuleset
        {
            get => _CurrentRuleset;
            set => _CurrentRuleset = value;
        }

        public string Activity
        {
            get => _Activity;
            set => _Activity = value;
        }

        private IDictionary<string, object> members;

        private string _Name = string.Empty;

        private int _GlobalRank;

        private int _RegionRank;

        private string _AvatarUrl;

        private int _PP;

        private string _CurrentRuleset;

        private string _Activity;

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

    public class UserInfoDBusService : IMDBusObject, IUserInfoDBusService
    {
        public ObjectPath ObjectPath => PATH;
        public static readonly ObjectPath PATH = new ObjectPath("/io/matrix_feather/mfosu/CurrentUser");

        public string CustomRegisterName => null;
        public bool IsService => true;

        public APIUser User
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

        private readonly TimeSpan loadTick = new TimeSpan(DateTime.Now.Ticks);

        private readonly UserMetadataProperties properties = new UserMetadataProperties();

        public Task<UserMetadataProperties> GetAllAsync()
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

        public Task<string> GetNameAsync()
        {
            return Task.FromResult(properties.Name);
        }

        public Task<string> GetActivityAsync()
        {
            return Task.FromResult(properties.Activity);
        }

        public Task<int> GetGlobalRankAsync()
        {
            return Task.FromResult(properties.GlobalRank);
        }

        public Task<int> GetRegionRankAsync()
        {
            return Task.FromResult(properties.RegionRank);
        }

        public Task<string> GetAvatarUrlAsync()
        {
            return Task.FromResult(properties.AvatarUrl);
        }

        public Task<int> GetPPAsync()
        {
            return Task.FromResult(properties.PP);
        }

        public Task<string> GetCurrentRulesetAsync()
        {
            return Task.FromResult(properties.CurrentRuleset);
        }

        public Task<long> GetLaunchTickAsync()
        {
            return Task.FromResult(new TimeSpan(DateTime.Now.Ticks).Ticks - loadTick.Ticks);
        }

        internal void SetProperty(string target, object value)
        {
            if (properties.Set(target, value))
                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty(target, value));
        }

        public event Action<PropertyChanges> OnPropertiesChanged;
    }
}
