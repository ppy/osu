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
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public partial class MigrationSelectScreen : DirectorySelectScreen
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

        public override LocalisableString HeaderText => MaintenanceSettingsStrings.SelectNewLocation;

        protected override void OnSelection(DirectoryInfo directory)
        {
            var target = directory;

            try
            {
                var directoryInfos = target.GetDirectories();
                var fileInfos = target.GetFiles();

                if (directoryInfos.Length > 0 || fileInfos.Length > 0 || target.Parent == null)
                {
                    // Quick test for whether there's already an osu! install at the target path.
                    if (fileInfos.Any(f => f.Name == OsuGameBase.CLIENT_DATABASE_FILENAME))
                    {
                        dialogOverlay.Push(new ConfirmDialog(MaintenanceSettingsStrings.TargetDirectoryAlreadyInstalledOsu, () =>
                            {
                                dialogOverlay.Push(new ConfirmDialog(MaintenanceSettingsStrings.RestartAndReOpenRequiredForCompletion, () =>
                                {
                                    (storage as OsuStorage)?.ChangeDataPath(target.FullName);
                                    game.Exit();
                                }, () => { }));
                            },
                            () => { }));

                        return;
                    }

                    // Not using CreateSubDirectory as it throws unexpectedly when attempting to create a directory when already at the root of a disk.
                    // See https://cs.github.com/dotnet/runtime/blob/f1bdd5a6182f43f3928b389b03f7bc26f826c8bc/src/libraries/System.Private.CoreLib/src/System/IO/DirectoryInfo.cs#L88-L94
                    target = Directory.CreateDirectory(Path.Combine(target.FullName, @"osu-lazer"));
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
