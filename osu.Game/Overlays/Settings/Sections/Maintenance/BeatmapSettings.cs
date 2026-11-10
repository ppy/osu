// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public partial class BeatmapSettings : SettingsSubsection
    {
        protected override LocalisableString Header => CommonStrings.Beatmaps;

        private SettingsButtonV2 deleteBeatmapsButton = null!;
        private SettingsButtonV2 deleteBeatmapVideosButton = null!;
        private SettingsButtonV2 resetOffsetsButton = null!;
        private SettingsButtonV2 restoreButton = null!;
        private SettingsButtonV2 undeleteButton = null!;

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmaps, IDialogOverlay? dialogOverlay)
        {
            Add(deleteBeatmapsButton = new DangerousSettingsButtonV2
            {
                Text = MaintenanceSettingsStrings.DeleteAllBeatmaps,
                Action = () =>
                {
                    dialogOverlay?.Push(new MassDeleteConfirmationDialog(() =>
                    {
                        deleteBeatmapsButton.Enabled.Value = false;
                        Task.Run(() => beatmaps.Delete()).ContinueWith(_ => Schedule(() => deleteBeatmapsButton.Enabled.Value = true));
                    }, DeleteConfirmationContentStrings.Beatmaps));
                }
            });

            Add(deleteBeatmapVideosButton = new DangerousSettingsButtonV2
            {
                Text = MaintenanceSettingsStrings.DeleteAllBeatmapVideos,
                Action = () =>
                {
                    dialogOverlay?.Push(new MassDeleteConfirmationDialog(() =>
                    {
                        deleteBeatmapVideosButton.Enabled.Value = false;
                        Task.Run(beatmaps.DeleteAllVideos).ContinueWith(_ => Schedule(() => deleteBeatmapVideosButton.Enabled.Value = true));
                    }, DeleteConfirmationContentStrings.BeatmapVideos));
                }
            });

            Add(resetOffsetsButton = new DangerousSettingsButtonV2
            {
                Text = MaintenanceSettingsStrings.ResetAllOffsets,
                Action = () =>
                {
                    dialogOverlay?.Push(new MassDeleteConfirmationDialog(() =>
                    {
                        resetOffsetsButton.Enabled.Value = false;
                        Task.Run(beatmaps.ResetAllOffsets).ContinueWith(_ => Schedule(() => resetOffsetsButton.Enabled.Value = true));
                    }, DeleteConfirmationContentStrings.Offsets));
                }
            });

            AddRange(new Drawable[]
            {
                restoreButton = new SettingsButtonV2
                {
                    Text = MaintenanceSettingsStrings.RestoreAllHiddenDifficulties,
                    Action = () =>
                    {
                        restoreButton.Enabled.Value = false;
                        Task.Run(beatmaps.RestoreAll).ContinueWith(_ => Schedule(() => restoreButton.Enabled.Value = true));
                    }
                },
                undeleteButton = new SettingsButtonV2
                {
                    Text = MaintenanceSettingsStrings.RestoreAllRecentlyDeletedBeatmaps,
                    Action = () =>
                    {
                        undeleteButton.Enabled.Value = false;
                        Task.Run(beatmaps.UndeleteAll).ContinueWith(_ => Schedule(() => undeleteButton.Enabled.Value = true));
                    }
                }
            });
        }
    }
}
