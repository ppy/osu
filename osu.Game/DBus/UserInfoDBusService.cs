using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Game.Users;
using Tmds.DBus;

namespace osu.Game.DBus
{
    [DBusInterface("io.matrix_feather.mfosu.CurrentUser")]
    public interface IUserInfoDBusService : IDBusObject
    {
        Task<IDictionary<string, object>> GetAllAsync();
        Task<object> GetAsync(string prop);
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);

        Task<string> GetUserNameAsync();
        Task<string> GetUserActivityAsync();
        Task<string> GetUserRankAsync();
        Task<string> GetUserCountryRegionRankAsync();
        Task<string> GetUserAvatarUrlAsync();
        Task<string> GetPPAsync();
        Task<string> GetCurrentRulesetAsync();
        Task<long> GetLaunchTickAsync();
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
                SetProperty("name", value.Username);
                SetProperty("global_rank", value.Statistics?.GlobalRank > 0 ? $"{value.Statistics.GlobalRank:N0}" : "-1");
                SetProperty("region_rank", value.Statistics?.CountryRank > 0 ? $"{value.Statistics.CountryRank:N0}" : "-1");
                SetProperty("avatar_url", value.AvatarUrl ?? string.Empty);
                SetProperty("pp", value.Statistics?.PP > 0 ? $"{value.Statistics.PP}" : "0");
            }
        }

        internal void SetProperty(string name, object value)
        {
            properties[name] = value;
            OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty(name, value));
        }

        private readonly IDictionary<string, object> properties = new ConcurrentDictionary<string, object>
        {
            ["name"] = string.Empty,
            ["global_rank"] = 0d,
            ["region_rank"] = 0,
            ["avatar_url"] = string.Empty,
            ["pp"] = 0,
            ["current_ruleset"] = string.Empty,
            ["activity"] = string.Empty //需要从外部修改？
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

        public Task<string> GetUserNameAsync()
            => Task.FromResult(properties["name"] as string);

        public Task<string> GetUserActivityAsync()
            => Task.FromResult(properties["activity"] as string);

        public Task<string> GetUserRankAsync()
            => Task.FromResult(properties["global_rank"] as string);

        public Task<string> GetUserCountryRegionRankAsync()
            => Task.FromResult(properties["region_rank"] as string);

        public Task<string> GetUserAvatarUrlAsync()
            => Task.FromResult(properties["avatar_url"] as string);

        public Task<string> GetPPAsync()
            => Task.FromResult(properties["pp"] as string);

        public Task<string> GetCurrentRulesetAsync()
            => Task.FromResult(properties["current_ruleset"] as string);

        public Task<long> GetLaunchTickAsync()
            => Task.FromResult(new TimeSpan(DateTime.Now.Ticks).Ticks - loadTick.Ticks);
    }
}
