using M.DBus;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class DBusSettings : SettingsSubsection
    {
        protected override LocalisableString Header => "DBus";

        [BackgroundDependencyLoader]
        private void load(MConfigManager config, DBusManager dBusManager, OsuColour osuColour)
        {
            SettingsCheckbox intergrationCheckbox;
            Children = new Drawable[]
            {
                intergrationCheckbox = new SettingsCheckbox
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
                }
            };

            if (RuntimeInfo.OS == RuntimeInfo.Platform.Linux)
            {
                SettingsCheckbox trayCheckbox;
                AddRange(new Drawable[]
                {
                    trayCheckbox = new SettingsCheckbox
                    {
                        LabelText = "启用DBus系统托盘",
                        Current = config.GetBindable<bool>(MSetting.EnableTray)
                    },
                    new SettingsCheckbox
                    {
                        LabelText = "允许通过DBus发送系统通知",
                        Current = config.GetBindable<bool>(MSetting.EnableSystemNotifications)
                    },
                    new SettingsTextBox
                    {
                        LabelText = "托盘图标名称",
                        Current = config.GetBindable<string>(MSetting.TrayIconName)
                    }
                });

                trayCheckbox.WarningText = "由于未知原因, 启用再禁用托盘功能不会使托盘图标消失。\n具体原因正在调查中。";
            }
            else
            {
                intergrationCheckbox.WarningText = "非Linux平台可能需要自行安装并启用DBus";
            }
        }
    }
}
