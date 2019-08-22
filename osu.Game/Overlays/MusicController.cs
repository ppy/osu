// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Input.Bindings;
using osu.Game.Overlays.OSD;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays
{
    /// <summary>
    /// Handles playback of the global music track.
    /// </summary>
    public class MusicController : Component, IKeyBindingHandler<GlobalAction>
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        private List<BeatmapSetInfo> beatmapSets;

        public bool IsUserPaused { get; private set; }

        /// <summary>
        /// Fired when the global <see cref="WorkingBeatmap"/> has changed.
        /// Includes direction information for display purposes.
        /// </summary>
        public event Action<WorkingBeatmap, TrackChangeDirection> TrackChanged;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; }

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; }

        [Resolved(canBeNull: true)]
        private OnScreenDisplay onScreenDisplay { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            beatmapSets = beatmaps.GetAllUsableBeatmapSets();
            beatmaps.ItemAdded += handleBeatmapAdded;
            beatmaps.ItemRemoved += handleBeatmapRemoved;
        }

        protected override void LoadComplete()
        {
            beatmap.BindValueChanged(beatmapChanged, true);
            mods.BindValueChanged(_ => updateAudioAdjustments(), true);
            base.LoadComplete();
        }

        /// <summary>
        /// Change the position of a <see cref="BeatmapSetInfo"/> in the current playlist.
        /// </summary>
        /// <param name="beatmapSetInfo">The beatmap to move.</param>
        /// <param name="index">The new position.</param>
        public void ChangeBeatmapSetPosition(BeatmapSetInfo beatmapSetInfo, int index)
        {
            beatmapSets.Remove(beatmapSetInfo);
            beatmapSets.Insert(index, beatmapSetInfo);
        }

        /// <summary>
        /// Returns whether the current beatmap track is playing.
        /// </summary>
        public bool IsPlaying => beatmap.Value.Track.IsRunning;

        private void handleBeatmapAdded(BeatmapSetInfo set) =>
            Schedule(() => beatmapSets.Add(set));

        private void handleBeatmapRemoved(BeatmapSetInfo set) =>
            Schedule(() => beatmapSets.RemoveAll(s => s.ID == set.ID));

        private ScheduledDelegate seekDelegate;

        public void SeekTo(double position)
        {
            seekDelegate?.Cancel();
            seekDelegate = Schedule(() =>
            {
                if (!beatmap.Disabled)
                    current?.Track.Seek(position);
            });
        }

        /// <summary>
        /// Start playing the current track (if not already playing).
        /// </summary>
        public void Play()
        {
            if (!IsPlaying)
                TogglePause();
        }

        /// <summary>
        /// Toggle pause / play.
        /// </summary>
        /// <returns>Whether the operation was successful.</returns>
        public bool TogglePause()
        {
            var track = current?.Track;

            if (track == null)
            {
                if (beatmap.Disabled)
                    return false;

                next(true);
                return true;
            }

            if (track.IsRunning)
            {
                IsUserPaused = true;
                track.Stop();
            }
            else
            {
                track.Start();
                IsUserPaused = false;
            }

            return true;
        }

        /// <summary>
        /// Play the previous track.
        /// </summary>
        /// <returns>Whether the operation was successful.</returns>
        public bool PrevTrack()
        {
            queuedDirection = TrackChangeDirection.Prev;

            var playable = beatmapSets.TakeWhile(i => i.ID != current.BeatmapSetInfo.ID).LastOrDefault() ?? beatmapSets.LastOrDefault();

            if (playable != null)
            {
                if (beatmap is Bindable<WorkingBeatmap> working)
                    working.Value = beatmaps.GetWorkingBeatmap(playable.Beatmaps.First(), beatmap.Value);
                beatmap.Value.Track.Restart();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Play the next random or playlist track.
        /// </summary>
        /// <returns>Whether the operation was successful.</returns>
        public bool NextTrack() => next();

        private bool next(bool instant = false)
        {
            if (!instant)
                queuedDirection = TrackChangeDirection.Next;

            var playable = beatmapSets.SkipWhile(i => i.ID != current.BeatmapSetInfo.ID).Skip(1).FirstOrDefault() ?? beatmapSets.FirstOrDefault();

            if (playable != null)
            {
                if (beatmap is Bindable<WorkingBeatmap> working)
                    working.Value = beatmaps.GetWorkingBeatmap(playable.Beatmaps.First(), beatmap.Value);
                beatmap.Value.Track.Restart();
                return true;
            }

            return false;
        }

        private WorkingBeatmap current;

        private TrackChangeDirection? queuedDirection;

        private void beatmapChanged(ValueChangedEvent<WorkingBeatmap> beatmap)
        {
            TrackChangeDirection direction = TrackChangeDirection.None;

            if (current != null)
            {
                bool audioEquals = beatmap.NewValue?.BeatmapInfo?.AudioEquals(current.BeatmapInfo) ?? false;

                if (audioEquals)
                    direction = TrackChangeDirection.None;
                else if (queuedDirection.HasValue)
                {
                    direction = queuedDirection.Value;
                    queuedDirection = null;
                }
                else
                {
                    //figure out the best direction based on order in playlist.
                    var last = beatmapSets.TakeWhile(b => b.ID != current.BeatmapSetInfo?.ID).Count();
                    var next = beatmap.NewValue == null ? -1 : beatmapSets.TakeWhile(b => b.ID != beatmap.NewValue.BeatmapSetInfo?.ID).Count();

                    direction = last > next ? TrackChangeDirection.Prev : TrackChangeDirection.Next;
                }
            }

            current = beatmap.NewValue;
            TrackChanged?.Invoke(current, direction);

            updateAudioAdjustments();

            queuedDirection = null;
        }

        private void updateAudioAdjustments()
        {
            var track = current?.Track;
            if (track == null)
                return;

            track.ResetSpeedAdjustments();

            foreach (var mod in mods.Value.OfType<IApplicableToClock>())
                mod.ApplyToClock(track);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (beatmaps != null)
            {
                beatmaps.ItemAdded -= handleBeatmapAdded;
                beatmaps.ItemRemoved -= handleBeatmapRemoved;
            }
        }

        public bool OnPressed(GlobalAction action)
        {
            if (beatmap.Disabled)
                return false;

            switch (action)
            {
                case GlobalAction.MusicPlay:
                    if (TogglePause())
                        onScreenDisplay?.Display(new MusicControllerToast(IsPlaying ? "Play track" : "Pause track"));
                    return true;

                case GlobalAction.MusicNext:
                    if (NextTrack())
                        onScreenDisplay?.Display(new MusicControllerToast("Next track"));

                    return true;

                case GlobalAction.MusicPrev:
                    if (PrevTrack())
                        onScreenDisplay?.Display(new MusicControllerToast("Previous track"));

                    return true;
            }

            return false;
        }

        public bool OnReleased(GlobalAction action) => false;

        public class MusicControllerToast : Toast
        {
            public MusicControllerToast(string action)
                : base("Music Playback", action, string.Empty)
            {
            }
        }
    }

    public enum TrackChangeDirection
    {
        None,
        Next,
        Prev
    }
}
