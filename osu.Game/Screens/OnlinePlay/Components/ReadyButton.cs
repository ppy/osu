// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public abstract class ReadyButton : TriangleButton
    {
        public readonly Bindable<PlaylistItem> SelectedItem = new Bindable<PlaylistItem>();

        public new readonly BindableBool Enabled = new BindableBool();

        [Resolved]
        protected IBindable<WorkingBeatmap> GameBeatmap { get; private set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        private bool hasBeatmap;

        private IBindable<WeakReference<BeatmapSetInfo>> managerUpdated;
        private IBindable<WeakReference<BeatmapSetInfo>> managerRemoved;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            managerUpdated = beatmaps.ItemUpdated.GetBoundCopy();
            managerUpdated.BindValueChanged(beatmapUpdated);
            managerRemoved = beatmaps.ItemRemoved.GetBoundCopy();
            managerRemoved.BindValueChanged(beatmapRemoved);

            SelectedItem.BindValueChanged(item => updateSelectedItem(item.NewValue), true);
        }

        private void updateSelectedItem(PlaylistItem _) => Scheduler.AddOnce(updateBeatmapState);
        private void beatmapUpdated(ValueChangedEvent<WeakReference<BeatmapSetInfo>> _) => Scheduler.AddOnce(updateBeatmapState);
        private void beatmapRemoved(ValueChangedEvent<WeakReference<BeatmapSetInfo>> _) => Scheduler.AddOnce(updateBeatmapState);

        private void updateBeatmapState()
        {
            int? beatmapId = SelectedItem.Value?.Beatmap.Value?.OnlineBeatmapID;
            string checksum = SelectedItem.Value?.Beatmap.Value?.MD5Hash;

            if (beatmapId == null || checksum == null)
                return;

            var databasedBeatmap = beatmaps.QueryBeatmap(b => b.OnlineBeatmapID == beatmapId && b.MD5Hash == checksum);

            hasBeatmap = databasedBeatmap?.BeatmapSet?.DeletePending == false;
        }

        protected override void Update()
        {
            base.Update();

            updateEnabledState();
        }

        private void updateEnabledState()
        {
            if (GameBeatmap.Value == null || SelectedItem.Value == null)
            {
                base.Enabled.Value = false;
                return;
            }

            base.Enabled.Value = hasBeatmap && Enabled.Value;
        }
    }
}
