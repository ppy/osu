// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Statistics;
using osu.Game.Graphics;
using osu.Game.IO;
using osu.Game.Localisation;
using osu.Game.Overlays.Notifications;
using osu.Game.Overlays.Settings.Sections.Maintenance;
using osu.Game.Utils;
using SharpCompress.Archives.Zip;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public partial class InstallationSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GeneralSettingsStrings.InstallationHeader;

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        [Resolved]
        private OsuGame? game { get; set; }

        private Storage exportStorage = null!;

        [BackgroundDependencyLoader]
        private void load(Storage storage, OsuColour colours)
        {
            bool isDesktop = RuntimeInfo.IsDesktop;
            bool supportsExport = RuntimeInfo.OS != RuntimeInfo.Platform.Android;

            // Loosely update-related maintenance buttons.
            if (isDesktop)
            {
                Add(new SettingsButton
                {
                    Text = GeneralSettingsStrings.OpenOsuFolder,
                    Keywords = new[] { @"logs", @"files", @"access", "directory" },
                    Action = () => storage.PresentExternally(),
                });

                Add(new DangerousSettingsButton
                {
                    Text = GeneralSettingsStrings.ChangeFolderLocation,
                    Action = () => game?.PerformFromScreen(menu => menu.Push(new MigrationSelectScreen()))
                });
            }

            if (supportsExport)
            {
                Add(new SettingsButton
                {
                    Text = GeneralSettingsStrings.ExportLogs,
                    BackgroundColour = colours.YellowDarker,
                    Keywords = new[] { @"bug", "report", "logs", "files" },
                    Action = () => Task.Run(exportLogs),
                });

                exportStorage = (storage as OsuStorage)?.GetExportStorage() ?? storage.GetStorageForDirectory(@"exports");
            }
        }

        private void exportLogs()
        {
            ProgressNotification notification = new ProgressNotification
            {
                State = ProgressNotificationState.Active,
                Text = "Exporting logs...",
            };

            notifications?.Post(notification);

            const string archive_filename = "compressed-logs.zip";

            try
            {
                GlobalStatistics.OutputToLog();
                Logger.Flush();

                var logStorage = Logger.Storage;

                using (var outStream = exportStorage.CreateFileSafely(archive_filename))
                using (var zip = ZipArchive.Create())
                {
                    foreach (string? f in logStorage.GetFiles(string.Empty, "*.log"))
                        FileUtils.AttemptOperation(z => z.AddEntry(f, logStorage.GetStream(f), true), zip);

                    zip.SaveTo(outStream);
                }
            }
            catch
            {
                notification.State = ProgressNotificationState.Cancelled;

                // cleanup if export is failed or canceled.
                exportStorage.Delete(archive_filename);
                throw;
            }

            notification.CompletionText = "Exported logs! Click to view.";
            notification.CompletionClickAction = () => exportStorage.PresentFileExternally(archive_filename);

            notification.State = ProgressNotificationState.Completed;
        }
    }
}
