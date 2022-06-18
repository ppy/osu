// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.IO;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class MigrationSelectScreen : DirectorySelectScreen
    {
        [Resolved]
        private Storage storage { get; set; }

        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved(canBeNull: true)]
        private IDialogOverlay dialogOverlay { get; set; }

        protected override DirectoryInfo InitialPath => new DirectoryInfo(storage.GetFullPath(string.Empty)).Parent;

        public override bool AllowExternalScreenChange => false;

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public override bool HideOverlaysOnEnter => true;

        public override LocalisableString HeaderText => "Please select a new location";

        protected override void OnSelection(DirectoryInfo directory)
        {
            var target = directory;

            try
            {
                var directoryInfos = target.GetDirectories();
                var fileInfos = target.GetFiles();

                if (directoryInfos.Length > 0 || fileInfos.Length > 0)
                {
                    // Quick test for whether there's already an osu! install at the target path.
                    if (fileInfos.Any(f => f.Name == OsuGameBase.CLIENT_DATABASE_FILENAME))
                    {
                        dialogOverlay.Push(new ConfirmDialog("The target directory already seems to have an osu! install. Use that data instead?", () =>
                            {
                                dialogOverlay.Push(new ConfirmDialog("To complete this operation, osu! will close. Please open it again to use the new data location.", () =>
                                {
                                    (storage as OsuStorage)?.ChangeDataPath(target.FullName);
                                    game.GracefullyExit();
                                }, () => { }));
                            },
                            () => { }));

                        return;
                    }

                    target = target.CreateSubdirectory("osu-lazer");
                }
            }
            catch (Exception e)
            {
                Logger.Log($"Error during migration: {e.Message}", level: LogLevel.Error);
                return;
            }

            ValidForResume = false;
            BeginMigration(target);
        }

        protected virtual void BeginMigration(DirectoryInfo target) => this.Push(new MigrationRunScreen(target));
    }
}
