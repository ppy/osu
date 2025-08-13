// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Notifications
{
    /// <summary>
    /// A notification showing the current server information on startup.
    /// </summary>
    public partial class ServerInfoNotification : SimpleNotification
    {
        private readonly string serverUrl;

        public ServerInfoNotification(string serverUrl)
        {
            this.serverUrl = serverUrl;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Icon = FontAwesome.Solid.Server;
            IconContent.Colour = colours.BlueLight;
            
            Text = GetServerDisplayText(serverUrl);
        }

        private static LocalisableString GetServerDisplayText(string serverUrl)
        {
            if (string.IsNullOrEmpty(serverUrl))
                return OnlineSettingsStrings.ConnectedToDefaultServer;

            // 提取主机名用于显示
            string displayName = ExtractDisplayName(serverUrl);
            return OnlineSettingsStrings.CurrentServer(displayName);
        }

        private static string ExtractDisplayName(string url)
        {
            if (string.IsNullOrEmpty(url))
                return OnlineSettingsStrings.DefaultServer.ToString();

            try
            {
                // 移除协议前缀
                string cleanUrl = url.Replace("https://", "").Replace("http://", "");
                
                // 移除路径部分
                int pathIndex = cleanUrl.IndexOf('/');
                if (pathIndex > 0)
                    cleanUrl = cleanUrl.Substring(0, pathIndex);

                // 检查是否为已知服务器
                switch (cleanUrl.ToLowerInvariant())
                {
                    case "osu.ppy.sh":
                        return OnlineSettingsStrings.OfficialServer.ToString();
                    case "dev.ppy.sh":
                        return OnlineSettingsStrings.DevelopmentServer.ToString();
                    default:
                        return cleanUrl;
                }
            }
            catch
            {
                return url;
            }
        }
    }
}
