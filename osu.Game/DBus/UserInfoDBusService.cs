using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Game.Rulesets;
using osu.Game.Users;
using Tmds.DBus;

namespace osu.Game.DBus
{
    [DBusInterface("io.matrix_feather.mfosu.CurrentUser")]
    public interface IUserInfoDBusService : IDBusObject
    {
        Task<string> GetUserNameAsync();
        Task<string> GetUserActivityAsync();
        Task<string> GetUserRankAsync();
        Task<string> GetUserCountryRegionRankAsync();
        Task<string> GetUserAvatarUrlAsync();
        Task<string> GetPPAsync();
        Task<string> GetCurrentRulesetAsync();
        Task<string> GetLaunchTimeAsync();
    }

    public class UserInfoDBusService : IUserInfoDBusService
    {
        public ObjectPath ObjectPath => PATH;
        public static readonly ObjectPath PATH = new ObjectPath("/io/matrix_feather/mfosu/CurrentUser");

        [NotNull]
        public User User { get; set; }

        public IBindable<RulesetInfo> Ruleset { get; set; }

        private readonly TimeSpan loadTick = new TimeSpan(DateTime.Now.Ticks);

        public Task<string> GetUserNameAsync()
        {
            return Task.FromResult(User.Username ?? "???");
        }

        public Task<string> GetUserActivityAsync()
        {
            return Task.FromResult(User.Activity.Value?.Status ?? "空闲");
        }

        public Task<string> GetUserRankAsync()
        {
            return Task.FromResult(User.Statistics?.GlobalRank > 0 ? $"{User.Statistics.GlobalRank:N0}" : "-1");
        }

        public Task<string> GetUserCountryRegionRankAsync()
        {
            return Task.FromResult(User.Statistics?.CountryRank > 0 ? $"{User.Statistics.CountryRank:N0}" : "-1");
        }

        public Task<string> GetUserAvatarUrlAsync()
            => Task.FromResult(User.AvatarUrl ?? string.Empty);

        public Task<string> GetPPAsync()
            => Task.FromResult(User.Statistics?.PP > 0 ? User.Statistics.PP.ToString() : "0");

        public Task<string> GetCurrentRulesetAsync()
            => Task.FromResult(Ruleset.Value?.Name ?? string.Empty);

        public Task<string> GetLaunchTimeAsync()
            => Task.FromResult(getLaunchTime());

        private string getLaunchTime()
        {
            string result = string.Empty;
            var currentTick = new TimeSpan(DateTime.Now.Ticks);
            var xE = currentTick.Subtract(loadTick);

            if (xE.Hours > 0)
                result += $"{(xE.Hours + xE.Days * 24):00}:";

            result += $"{xE.Minutes:00}:{xE.Seconds:00}";
            return result;
        }
    }
}
