// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Statistics;
using osu.Game.Graphics;
using osu.Game.IO;
using osu.Game.Localisation;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Notifications;
using osu.Game.Utils;
using SharpCompress.Archives.Zip;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public partial class QuickActionSettings : SettingsSubsection
    {
        [Resolved(CanBeNull = true)]
        private FirstRunSetupOverlay? firstRunSetupOverlay { get; set; }

        [Resolved(CanBeNull = true)]
        private OsuGame? game { get; set; }

        protected override LocalisableString Header => GeneralSettingsStrings.QuickActionsHeader;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, Storage storage)
        {
            AddRange(new Drawable[]
            {
                new SettingsButton
                {
                    Text = GeneralSettingsStrings.RunSetupWizard,
                    Keywords = new[] { @"first run", @"initial", @"getting started", @"import", @"tutorial", @"recommended beatmaps" },
                    TooltipText = FirstRunSetupOverlayStrings.FirstRunSetupDescription,
                    Action = () => firstRunSetupOverlay?.Show(),
                },
                new SettingsButton
                {
                    Text = GeneralSettingsStrings.LearnMoreAboutLazer,
                    TooltipText = GeneralSettingsStrings.LearnMoreAboutLazerTooltip,
                    BackgroundColour = colours.YellowDark,
                    Action = () => game?.ShowWiki(@"Help_centre/Upgrading_to_lazer")
                },
                new SettingsButton
                {
                    Text = GeneralSettingsStrings.ReportIssue,
                    TooltipText = GeneralSettingsStrings.ReportIssueTooltip,
                    BackgroundColour = colours.YellowDarker,
                    Action = () => game?.OpenUrlExternally(@"https://osu.ppy.sh/community/forums/topics/create?forum_id=5", LinkWarnMode.NeverWarn)
                },
            });

            bool supportsExport = RuntimeInfo.OS != RuntimeInfo.Platform.Android;

            if (supportsExport)
            {
                Add(new SettingsButton
                {
                    Text = GeneralSettingsStrings.ExportLogs,
                    BackgroundColour = colours.YellowDarker.Darken(0.5f),
                    Keywords = new[] { @"bug", "report", "logs", "files" },
                    Action = () => Task.Run(exportLogs),
                });

                exportStorage = (storage as OsuStorage)?.GetExportStorage() ?? storage.GetStorageForDirectory(@"exports");
            }
        }

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        private Storage exportStorage = null!;

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
