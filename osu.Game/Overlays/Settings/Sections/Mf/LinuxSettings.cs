// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using M.DBus;
using M.DBus.Services.Notifications;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class LinuxSettings : SettingsSubsection
    {
        private SettingsCheckbox trayCheckbox;
        protected override LocalisableString Header => "Linux集成";

        [BackgroundDependencyLoader]
        private void load(MConfigManager config, DBusManager dBusManager)
        {
            Children = new Drawable[]
            {
                new SettingsEnumDropdown<GamemodeActivateCondition>
                {
                    LabelText = "Gamemode启用条件",
                    TooltipText = "依赖libgamemode",
                    Current = config.GetBindable<GamemodeActivateCondition>(MSetting.Gamemode)
                },
                new SettingsCheckbox
                {
                    LabelText = "D-Bus集成",
                    Current = config.GetBindable<bool>(MSetting.DBusIntegration)
                },
                new SettingsCheckbox
                {
                    LabelText = "允许通过D-Bus发送消息到游戏内",
                    Current = config.GetBindable<bool>(MSetting.DBusAllowPost)
                },
                new SettingsCheckbox
                {
                    LabelText = "总是使用avatarlogo作为mpris封面",
                    Current = config.GetBindable<bool>(MSetting.MprisUseAvatarlogoAsCover)
                },
                trayCheckbox = new SettingsCheckbox
                {
                    LabelText = "启用DBus系统托盘",
                    Current = config.GetBindable<bool>(MSetting.EnableTray)
                },
                new SettingsCheckbox
                {
                    LabelText = "允许发送系统通知",
                    Current = config.GetBindable<bool>(MSetting.EnableSystemNotifications)
                },
                new SettingsButton
                {
                    Text = "发送！",
                    Action = () =>
                    {
                        dBusManager.Notifications.PostAsync(new SystemNotification
                        {
                            Name = "设置",
                            Title = "标题",
                            Description = "内容: https://bilibili.com",
                            Actions = new[]
                            {
                                new NotificationAction("OK", "好", () => Logger.Log("OK被调用！")),
                                new NotificationAction("NO", "不要！", () => Logger.Log("NO被调用！")),
                                new NotificationAction("PENDING", "我再想想", () => Logger.Log("PENDING被调用！")),
                            },
                            OnClosed = v =>
                            {
                                Logger.Log($"通知关闭！{v}");
                            }
                        });
                    }
                }
            };

            trayCheckbox.WarningText = "由于未知原因, 启用再禁用托盘功能不会使托盘图标消失。\n具体原因正在调查中。";
        }
    }
}
