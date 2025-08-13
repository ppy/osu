// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Text.RegularExpressions;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Localisation;


namespace osu.Game.Overlays.Settings.Sections.Online
{
    public partial class WebSettings : SettingsSubsection
    {
        protected override LocalisableString Header => OnlineSettingsStrings.WebHeader;

        [Resolved]
        private OsuGame? game { get; set; }

        private SettingsTextBox customApiUrlTextBox = null!;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = OnlineSettingsStrings.ExternalLinkWarning,
                    Current = config.GetBindable<bool>(OsuSetting.ExternalLinkWarning)
                },
                new SettingsCheckbox
                {
                    LabelText = OnlineSettingsStrings.PreferNoVideo,
                    Keywords = new[] { "no-video" },
                    Current = config.GetBindable<bool>(OsuSetting.PreferNoVideo)
                },
                new SettingsCheckbox
                {
                    LabelText = OnlineSettingsStrings.AutomaticallyDownloadMissingBeatmaps,
                    Keywords = new[] { "spectator", "replay" },
                    Current = config.GetBindable<bool>(OsuSetting.AutomaticallyDownloadMissingBeatmaps),
                },
                new SettingsCheckbox
                {
                    LabelText = OnlineSettingsStrings.ShowExplicitContent,
                    Keywords = new[] { "nsfw", "18+", "offensive" },
                    Current = config.GetBindable<bool>(OsuSetting.ShowOnlineExplicitContent),
                },
                customApiUrlTextBox = new SettingsTextBox
                {
                    LabelText = OnlineSettingsStrings.CustomApiUrl,
                    Current = config.GetBindable<string>(OsuSetting.CustomApiUrl)
                }
            };

            customApiUrlTextBox.Current.BindValueChanged(onCustomApiUrlChanged, true);
        }

        private string lastApiUrl = string.Empty; // last accepted normalized value: "" or "https://host[:port]"
        private bool isInitialLoad = true;
        private ScheduledDelegate? pendingValidation;
        private const double debounce_delay = 500;

        // Host[:port]（不含 scheme/path）的校验：
        // - 多级域名：至少一处点，例如 example.com、api.example.co.uk
        // - IPv4：每段 0–255
        // - 端口：可选，范围 1–65535（在正则后做数值校验）
        private static readonly Regex hostPortPattern = new Regex(
            pattern:
                @"^(?:" +
                    @"(?:(?:[A-Za-z0-9-]+)\.)+[A-Za-z0-9-]+" +                                  // multi-level domain (at least one dot)
                @"|" +
                    @"(?:(?:25[0-5]|2[0-4]\d|1?\d{1,2})\.){3}(?:25[0-5]|2[0-4]\d|1?\d{1,2})" +   // IPv4 0-255
                @")(?::(?<port>\d{1,5}))?$",
            options: RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private void onCustomApiUrlChanged(ValueChangedEvent<string> e)
        {
            if (isInitialLoad)
            {
                var initRaw = (e.NewValue ?? string.Empty).Trim();
                lastApiUrl = normalizeToHttps(initRaw);
                isInitialLoad = false;
                return;
            }

            pendingValidation?.Cancel();
            pendingValidation = Scheduler.AddDelayed(() =>
            {
                string rawInput = (customApiUrlTextBox.Current.Value ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(rawInput))
                {
                    maybeShowRestartIfChanged(string.Empty);
                    customApiUrlTextBox.SetNoticeText(string.Empty, false);
                    return;
                }
                string hostPort = stripSchemeAndPath(rawInput);
                if (!isValidHostPort(hostPort))
                {
                    customApiUrlTextBox.SetNoticeText(OnlineSettingsStrings.CustomApiUrlInvalid, true);
                    return;
                }
                string normalised = "https://" + hostPort;
                customApiUrlTextBox.SetNoticeText(string.Empty, false);
                maybeShowRestartIfChanged(normalised);
            }, debounce_delay);
        }

        private static bool isValidHostPort(string hostPort)
        {
            var m = hostPortPattern.Match(hostPort);
            if (!m.Success) return false;

            // 校验端口范围（如果提供）
            var g = m.Groups["port"];
            if (g.Success)
            {
                if (!int.TryParse(g.Value, out int port)) return false;
                if (port < 1 || port > 65535) return false;
            }
            return true;
        }

        // 规范化成用于比较/保存的值：
        // - 空 => ""
        // - 其他 => "https://host[:port]"（自动剥离 scheme/path, 校验通过才返回 https 形式；否则返回原始去除路径后的值）
        private static string normalizeToHttps(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            string hostPort = stripSchemeAndPath(raw);
            return isValidHostPort(hostPort) ? "https://" + hostPort : hostPort;
        }

        /// <summary>
        /// 去掉 http(s):// 前缀；从第一个 / 起截断（去路径/查询/fragment）；去掉末尾 /。
        /// </summary>
        private static string stripSchemeAndPath(string input)
        {
            string s = Regex.Replace(input, @"^\s*https?://", "", RegexOptions.IgnoreCase);

            int slash = s.IndexOf('/');
            if (slash >= 0) s = s.Substring(0, slash);

            s = s.TrimEnd('/');

            return s;
        }

        /// <summary>
        /// 若与上次接受值不同，则更新 lastApiUrl 并在文本框下提示需要重启；不再弹窗。
        /// </summary>
        private void maybeShowRestartIfChanged(string normalizedNewValue)
        {
            if (!string.Equals(lastApiUrl, normalizedNewValue, StringComparison.OrdinalIgnoreCase))
            {
                lastApiUrl = normalizedNewValue;

                // 仅提示，不弹框、不自动重启。
                // 使用非错误样式（第二参数 false）。
                customApiUrlTextBox.SetNoticeText(OnlineSettingsStrings.CustomApiUrlRestartRequired, false);
            }
        }
    }
}
