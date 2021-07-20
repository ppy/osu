// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Overlays.Notifications;
using osu.Game.Overlays.Settings.Sections.Maintenance;
using osu.Game.Updater;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public class UpdateSettings : SettingsSubsection
    {
        [Resolved(CanBeNull = true)]
        private UpdateManager updateManager { get; set; }

        protected override LocalisableString Header => "更新";

        private SettingsButton checkForUpdatesButton;

        [Resolved(CanBeNull = true)]
        private NotificationOverlay notifications { get; set; }

        [BackgroundDependencyLoader(true)]
        private void load(Storage storage, OsuConfigManager config, OsuGame game)
        {
            Add(new SettingsEnumDropdown<ReleaseStream>
            {
                LabelText = "更新频道",
                Current = config.GetBindable<ReleaseStream>(OsuSetting.ReleaseStream),
            });

            if (updateManager?.CanCheckForUpdate == true)
            {
                Add(checkForUpdatesButton = new SettingsButton
                {
                    Text = "检查更新",
                    Action = () =>
                    {
                        checkForUpdatesButton.Enabled.Value = false;
                        Task.Run(updateManager.CheckForUpdateAsync).ContinueWith(t => Schedule(() =>
                        {
                            if (!t.Result)
                            {
                                notifications?.Post(new SimpleNotification
                                {
                                    Text = $"已是最新版本! ({game.Version})",
                                    Icon = FontAwesome.Solid.CheckCircle,
                                });
                            }

                            checkForUpdatesButton.Enabled.Value = true;
                        }));
                    }
                });
            }

            if (RuntimeInfo.IsDesktop)
            {
                Add(new SettingsButton
                {
                    Text = "打开osu!文件夹",
                    Action = storage.OpenInNativeExplorer,
                });

                Add(new SettingsButton
                {
                    Text = "数据迁移",
                    Action = () => game?.PerformFromScreen(menu => menu.Push(new MigrationSelectScreen()))
                });
            }
        }
    }
}
