// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Game.Configuration;

namespace osu.Game.Online
{
    public sealed class TrustedDomainOnlineStore : OnlineStore
    {
        private readonly OsuConfigManager? configManager;

        public TrustedDomainOnlineStore(OsuConfigManager? configManager = null)
        {
            this.configManager = configManager;
        }

        protected override string GetLookupUrl(string url)
        {
            // 若配置了自定义 API URL，则放行所有请求（用于开发或自定义服务器）
            string? customApiUrl = configManager?.Get<string>(OsuSetting.CustomApiUrl);
            if (!string.IsNullOrWhiteSpace(customApiUrl))
                return url;

            // 校验是否是以 ".ppy.sh" 结尾的绝对 URL
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                uri.Host.EndsWith(".ppy.sh", StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }

            // 非法域名，阻止并记录日志
            Logger.Log(
                $"[TrustedDomainOnlineStore] Blocked external resource lookup: {url}",
                LoggingTarget.Network,
                LogLevel.Important
            );

            return string.Empty;
        }
    }
}
