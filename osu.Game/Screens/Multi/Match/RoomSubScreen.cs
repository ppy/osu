// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.Multi.Match
{
    [Cached(typeof(IPreviewTrackOwner))]
    public abstract class RoomSubScreen : MultiplayerSubScreen, IPreviewTrackOwner
    {
        protected readonly Bindable<PlaylistItem> SelectedItem = new Bindable<PlaylistItem>();

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        [Resolved(typeof(Room), nameof(Room.Playlist))]
        protected BindableList<PlaylistItem> Playlist { get; private set; }

        [Resolved]
        private MusicController music { get; set; }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        private IBindable<WeakReference<BeatmapSetInfo>> managerUpdated;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SelectedItem.BindValueChanged(_ => Scheduler.AddOnce(selectedItemChanged));
            SelectedItem.Value = Playlist.FirstOrDefault();

            managerUpdated = beatmapManager.ItemUpdated.GetBoundCopy();
            managerUpdated.BindValueChanged(beatmapUpdated);

            if (music != null)
                music.TrackChanged += applyToTrack;
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);
            applyToTrack();
        }

        public override void OnSuspending(IScreen next)
        {
            resetTrack();
            base.OnSuspending(next);
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);
            applyToTrack();
        }

        public override bool OnExiting(IScreen next)
        {
            RoomManager?.PartRoom();
            Mods.Value = Array.Empty<Mod>();

            resetTrack();

            return base.OnExiting(next);
        }

        private void selectedItemChanged()
        {
            updateWorkingBeatmap();

            var item = SelectedItem.Value;

            Mods.Value = item?.RequiredMods?.ToArray() ?? Array.Empty<Mod>();

            if (item?.Ruleset != null)
                Ruleset.Value = item.Ruleset.Value;
        }

        private void beatmapUpdated(ValueChangedEvent<WeakReference<BeatmapSetInfo>> weakSet) => Schedule(updateWorkingBeatmap);

        private void updateWorkingBeatmap()
        {
            var beatmap = SelectedItem.Value?.Beatmap.Value;

            // Retrieve the corresponding local beatmap, since we can't directly use the playlist's beatmap info
            var localBeatmap = beatmap == null ? null : beatmapManager.QueryBeatmap(b => b.OnlineBeatmapID == beatmap.OnlineBeatmapID);

            Beatmap.Value = beatmapManager.GetWorkingBeatmap(localBeatmap);
        }

        private void applyToTrack(WorkingBeatmap _ = default, TrackChangeDirection __ = default)
        {
            if (!this.IsCurrentScreen())
                return;

            var track = Beatmap.Value?.Track;

            if (track != null)
            {
                track.RestartPoint = Beatmap.Value.Metadata.PreviewTime;
                track.Looping = true;

                music?.EnsurePlayingSomething();
            }
        }

        private void resetTrack()
        {
            var track = Beatmap?.Value?.Track;

            if (track != null)
            {
                track.Looping = false;
                track.RestartPoint = 0;
            }
        }
    }
}
