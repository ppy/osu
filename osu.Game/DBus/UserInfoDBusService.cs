using System.Threading.Tasks;
using JetBrains.Annotations;
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
    }

    public class UserInfoDBusService : IUserInfoDBusService
    {
        public ObjectPath ObjectPath => PATH;
        public static readonly ObjectPath PATH = new ObjectPath("/io/matrix_feather/mfosu/CurrentUser");

        [NotNull]
        public User User { get; set; }

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
            => Task.FromResult(User.AvatarUrl ?? "???");
    }
}
