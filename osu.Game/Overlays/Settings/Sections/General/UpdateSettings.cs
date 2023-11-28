// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Overlays.Notifications;
using osu.Game.Overlays.Settings.Sections.Maintenance;
using osu.Game.Updater;
using SharpCompress.Archives.Zip;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public partial class UpdateSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GeneralSettingsStrings.UpdateHeader;

        private SettingsButton checkForUpdatesButton = null!;

        [Resolved]
        private UpdateManager? updateManager { get; set; }

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        [BackgroundDependencyLoader]
        private void load(Storage storage, OsuConfigManager config, OsuGame game)
        {
            Add(new SettingsEnumDropdown<ReleaseStream>
            {
                LabelText = GeneralSettingsStrings.ReleaseStream,
                Current = config.GetBindable<ReleaseStream>(OsuSetting.ReleaseStream),
            });

            if (updateManager?.CanCheckForUpdate == true)
            {
                Add(checkForUpdatesButton = new SettingsButton
                {
                    Text = GeneralSettingsStrings.CheckUpdate,
                    Action = () =>
                    {
                        checkForUpdatesButton.Enabled.Value = false;
                        Task.Run(updateManager.CheckForUpdateAsync).ContinueWith(task => Schedule(() =>
                        {
                            if (!task.GetResultSafely())
                            {
                                notifications?.Post(new SimpleNotification
                                {
                                    Text = GeneralSettingsStrings.RunningLatestRelease(game.Version),
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
                    Text = GeneralSettingsStrings.OpenOsuFolder,
                    Keywords = new[] { @"logs", @"files", @"access", "directory" },
                    Action = () => storage.PresentExternally(),
                });

                Add(new SettingsButton
                {
                    Text = GeneralSettingsStrings.ExportLogs,
                    Keywords = new[] { @"bug", "report", "logs", "files" },
                    Action = () =>
                    {
                        var logStorage = Logger.Storage;

                        const string archive_filename = "exports/compressed-logs.zip";

                        using (var outStream = storage.CreateFileSafely(archive_filename))
                        using (var zip = ZipArchive.Create())
                        {
                            foreach (string? f in logStorage.GetFiles(string.Empty, "*.log"))
                                zip.AddEntry(f, logStorage.GetStream(f), true);

                            zip.SaveTo(outStream);
                        }

                        storage.PresentFileExternally(archive_filename);
                    },
                });

                Add(new SettingsButton
                {
                    Text = GeneralSettingsStrings.ChangeFolderLocation,
                    Action = () => game.PerformFromScreen(menu => menu.Push(new MigrationSelectScreen()))
                });
            }
        }
    }
}
