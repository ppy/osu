using System.Threading.Tasks;
using M.DBus.Services.Notifications;

namespace M.DBus.Services
{
    public interface IHandleSystemNotifications
    {
        /// <summary>
        /// 向系统发送一条通知
        /// </summary>
        /// <param name="notification">要发送的通知</param>
        /// <returns>
        /// 此通知的id<br/>
        /// 如果通知功能被禁用，则会返回uint.MaxValue
        /// </returns>
        Task<uint> PostAsync(SystemNotification notification);

        /// <summary>
        /// 关闭一条通知
        /// </summary>
        /// <param name="id">通知的id号</param>
        /// <returns>
        /// true: 通知功能已启用<br/>
        /// false: 通知功能被禁用
        /// </returns>
        Task<bool> CloseNotificationAsync(uint id);

        /// <summary>
        /// 获取当前服务器支持的可选功能<br/>
        /// https://specifications.freedesktop.org/notification-spec/latest/ar01s09.html
        /// </summary>
        /// <returns>服务器支持的可选功能</returns>
        Task<string[]> GetCapabilitiesAsync();

        /// <summary>
        /// 获取当前服务器信息
        /// </summary>
        /// <returns>
        /// name: 服务器名<br/>
        /// vendor: 服务器供应商名称, 例如"GNOME"、"KDE"等<br/>
        /// version: 服务器自身的版本<br/>
        /// specVersion: 服务器符合的规范版本
        /// </returns>
        Task<(string name, string vendor, string version, string specVersion)> GetServerInformationAsync();
    }
}
