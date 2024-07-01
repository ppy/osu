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

        private SettingsButton deleteBeatmapsButton = null!;
        private SettingsButton deleteBeatmapVideosButton = null!;
        private SettingsButton restoreButton = null!;
        private SettingsButton undeleteButton = null!;

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmaps, IDialogOverlay? dialogOverlay)
        {
            Add(deleteBeatmapsButton = new DangerousSettingsButton
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

            Add(deleteBeatmapVideosButton = new DangerousSettingsButton
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
            AddRange(new Drawable[]
            {
                restoreButton = new SettingsButton
                {
                    Text = MaintenanceSettingsStrings.RestoreAllHiddenDifficulties,
                    Action = () =>
                    {
                        restoreButton.Enabled.Value = false;
                        Task.Run(beatmaps.RestoreAll).ContinueWith(_ => Schedule(() => restoreButton.Enabled.Value = true));
                    }
                },
                undeleteButton = new SettingsButton
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
