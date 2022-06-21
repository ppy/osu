// Copyright (c) ppy Pty Ltd <contact@ppy.sh>.Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class BeatmapSettings : SettingsSubsection
    {
        protected override LocalisableString Header => "Beatmaps";
        private SettingsButton importBeatmapsButton;
        private SettingsButton deleteBeatmapsButton;
        private SettingsButton deleteBeatmapVideosButton;
        private SettingsButton restoreButton;
        private SettingsButton undeleteButton;

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(BeatmapManager beatmaps, [CanBeNull] LegacyImportManager legacyImportManager, IDialogOverlay dialogOverlay)
        {
            if (legacyImportManager?.SupportsImportFromStable == true)
            {
                Add(importBeatmapsButton = new SettingsButton
                {
                    Text = MaintenanceSettingsStrings.ImportBeatmapsFromStable,
                    Action = () =>
                    {
                        importBeatmapsButton.Enabled.Value = false;
                        legacyImportManager.ImportFromStableAsync(StableContent.Beatmaps).ContinueWith(t => Schedule(() => importBeatmapsButton.Enabled.Value = true));
                    }
                });

                Add(deleteBeatmapsButton = new DangerousSettingsButton
                {
                    Text = MaintenanceSettingsStrings.DeleteAllBeatmaps,
                    Action = () =>
                    {
                        dialogOverlay?.Push(new MassDeleteConfirmationDialog(() =>
                        {
                            deleteBeatmapsButton.Enabled.Value = false;
                            Task.Run(() => beatmaps.Delete()).ContinueWith(t => Schedule(() => deleteBeatmapsButton.Enabled.Value = true));
                        }));
                    }
                });

                Add(deleteBeatmapVideosButton = new DangerousSettingsButton
                {
                    Text = MaintenanceSettingsStrings.DeleteAllBeatmapVideos,
                    Action = () =>
                    {
                        dialogOverlay?.Push(new MassVideoDeleteConfirmationDialog(() =>
                        {
                            deleteBeatmapVideosButton.Enabled.Value = false;
                            Task.Run(beatmaps.DeleteAllVideos).ContinueWith(t => Schedule(() => deleteBeatmapVideosButton.Enabled.Value = true));
                        }));
                    }
                });
                AddRange(new Drawable[]
                {
                    restoreButton = new SettingsButton
                    {
                        Text = MaintenanceSettingsStrings.RestoreAllHiddenDifficulties,
                        Action = () =>
                        {
                            restoreButton.Enabled.Value = false;
                            Task.Run(beatmaps.RestoreAll).ContinueWith(t => Schedule(() => restoreButton.Enabled.Value = true));
                        }
                    },
                    undeleteButton = new SettingsButton
                    {
                        Text = MaintenanceSettingsStrings.RestoreAllRecentlyDeletedBeatmaps,
                        Action = () =>
                        {
                            undeleteButton.Enabled.Value = false;
                            Task.Run(beatmaps.UndeleteAll).ContinueWith(t => Schedule(() => undeleteButton.Enabled.Value = true));
                        }
                    }
                });
            }
        }
    }
}
