// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Utils;
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
    public class MusicController : CompositeDrawable, IKeyBindingHandler<GlobalAction>
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        public IBindableList<BeatmapSetInfo> BeatmapSets
        {
            get
            {
                if (LoadState < LoadState.Ready)
                    throw new InvalidOperationException($"{nameof(BeatmapSets)} should not be accessed before the music controller is loaded.");

                return beatmapSets;
            }
        }

        /// <summary>
        /// Point in time after which the current track will be restarted on triggering a "previous track" action.
        /// </summary>
        private const double restart_cutoff_point = 5000;

        private readonly BindableList<BeatmapSetInfo> beatmapSets = new BindableList<BeatmapSetInfo>();

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

        [NotNull]
        public DrawableTrack CurrentTrack { get; private set; } = new DrawableTrack(new TrackVirtual(1000));

        private IBindable<WeakReference<BeatmapSetInfo>> managerUpdated;
        private IBindable<WeakReference<BeatmapSetInfo>> managerRemoved;

        [BackgroundDependencyLoader]
        private void load()
        {
            managerUpdated = beatmaps.ItemUpdated.GetBoundCopy();
            managerUpdated.BindValueChanged(beatmapUpdated);
            managerRemoved = beatmaps.ItemRemoved.GetBoundCopy();
            managerRemoved.BindValueChanged(beatmapRemoved);

            beatmapSets.AddRange(beatmaps.GetAllUsableBeatmapSets(IncludedDetails.Minimal, true).OrderBy(_ => RNG.Next()));

            // Todo: These binds really shouldn't be here, but are unlikely to cause any issues for now.
            // They are placed here for now since some tests rely on setting the beatmap _and_ their hierarchies inside their load(), which runs before the MusicController's load().
            beatmap.BindValueChanged(beatmapChanged, true);
            mods.BindValueChanged(_ => ResetTrackAdjustments(), true);
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
        /// Returns whether the beatmap track is playing.
        /// </summary>
        public bool IsPlaying => CurrentTrack.IsRunning;

        /// <summary>
        /// Returns whether the beatmap track is loaded.
        /// </summary>
        public bool TrackLoaded => CurrentTrack.TrackLoaded;

        private void beatmapUpdated(ValueChangedEvent<WeakReference<BeatmapSetInfo>> weakSet)
        {
            if (weakSet.NewValue.TryGetTarget(out var set))
            {
                Schedule(() =>
                {
                    beatmapSets.Remove(set);
                    beatmapSets.Add(set);
                });
            }
        }

        private void beatmapRemoved(ValueChangedEvent<WeakReference<BeatmapSetInfo>> weakSet)
        {
            if (weakSet.NewValue.TryGetTarget(out var set))
            {
                Schedule(() =>
                {
                    beatmapSets.RemoveAll(s => s.ID == set.ID);
                });
            }
        }

        private ScheduledDelegate seekDelegate;

        public void SeekTo(double position)
        {
            seekDelegate?.Cancel();
            seekDelegate = Schedule(() =>
            {
                if (!beatmap.Disabled)
                    CurrentTrack.Seek(position);
            });
        }

        /// <summary>
        /// Ensures music is playing, no matter what, unless the user has explicitly paused.
        /// This means that if the current beatmap has a virtual track (see <see cref="TrackVirtual"/>) a new beatmap will be selected.
        /// </summary>
        public void EnsurePlayingSomething()
        {
            if (IsUserPaused) return;

            if (CurrentTrack.IsDummyDevice)
            {
                if (beatmap.Disabled)
                    return;

                NextTrack();
            }
            else if (!IsPlaying)
            {
                Play();
            }
        }

        /// <summary>
        /// Start playing the current track (if not already playing).
        /// </summary>
        /// <returns>Whether the operation was successful.</returns>
        public bool Play(bool restart = false)
        {
            IsUserPaused = false;

            if (restart)
                CurrentTrack.Restart();
            else if (!IsPlaying)
                CurrentTrack.Start();

            return true;
        }

        /// <summary>
        /// Stop playing the current track and pause at the current position.
        /// </summary>
        public void Stop()
        {
            IsUserPaused = true;
            if (CurrentTrack.IsRunning)
                CurrentTrack.Stop();
        }

        /// <summary>
        /// Toggle pause / play.
        /// </summary>
        /// <returns>Whether the operation was successful.</returns>
        public bool TogglePause()
        {
            if (CurrentTrack.IsRunning)
                Stop();
            else
                Play();

            return true;
        }

        /// <summary>
        /// Play the previous track or restart the current track if it's current time below <see cref="restart_cutoff_point"/>.
        /// </summary>
        public void PreviousTrack() => Schedule(() => prev());

        /// <summary>
        /// Play the previous track or restart the current track if it's current time below <see cref="restart_cutoff_point"/>.
        /// </summary>
        /// <returns>The <see cref="PreviousTrackResult"/> that indicate the decided action.</returns>
        private PreviousTrackResult prev()
        {
            if (beatmap.Disabled)
                return PreviousTrackResult.None;

            var currentTrackPosition = CurrentTrack.CurrentTime;

            if (currentTrackPosition >= restart_cutoff_point)
            {
                SeekTo(0);
                return PreviousTrackResult.Restart;
            }

            queuedDirection = TrackChangeDirection.Prev;

            var playable = BeatmapSets.TakeWhile(i => i.ID != current.BeatmapSetInfo.ID).LastOrDefault() ?? BeatmapSets.LastOrDefault();

            if (playable != null)
            {
                if (beatmap is Bindable<WorkingBeatmap> working)
                    working.Value = beatmaps.GetWorkingBeatmap(playable.Beatmaps.First(), beatmap.Value);

                restartTrack();
                return PreviousTrackResult.Previous;
            }

            return PreviousTrackResult.None;
        }

        /// <summary>
        /// Play the next random or playlist track.
        /// </summary>
        public void NextTrack() => Schedule(() => next());

        private bool next()
        {
            if (beatmap.Disabled)
                return false;

            queuedDirection = TrackChangeDirection.Next;

            var playable = BeatmapSets.SkipWhile(i => i.ID != current.BeatmapSetInfo.ID).ElementAtOrDefault(1) ?? BeatmapSets.FirstOrDefault();

            if (playable != null)
            {
                if (beatmap is Bindable<WorkingBeatmap> working)
                    working.Value = beatmaps.GetWorkingBeatmap(playable.Beatmaps.First(), beatmap.Value);

                restartTrack();
                return true;
            }

            return false;
        }

        private void restartTrack()
        {
            // if not scheduled, the previously track will be stopped one frame later (see ScheduleAfterChildren logic in GameBase).
            // we probably want to move this to a central method for switching to a new working beatmap in the future.
            Schedule(() => CurrentTrack.Restart());
        }

        private WorkingBeatmap current;

        private TrackChangeDirection? queuedDirection;

        private void beatmapChanged(ValueChangedEvent<WorkingBeatmap> beatmap)
        {
            TrackChangeDirection direction = TrackChangeDirection.None;

            bool audioEquals = beatmap.NewValue?.BeatmapInfo?.AudioEquals(current?.BeatmapInfo) ?? false;

            if (current != null)
            {
                if (audioEquals)
                    direction = TrackChangeDirection.None;
                else if (queuedDirection.HasValue)
                {
                    direction = queuedDirection.Value;
                    queuedDirection = null;
                }
                else
                {
                    // figure out the best direction based on order in playlist.
                    var last = BeatmapSets.TakeWhile(b => b.ID != current.BeatmapSetInfo?.ID).Count();
                    var next = beatmap.NewValue == null ? -1 : BeatmapSets.TakeWhile(b => b.ID != beatmap.NewValue.BeatmapSetInfo?.ID).Count();

                    direction = last > next ? TrackChangeDirection.Prev : TrackChangeDirection.Next;
                }
            }

            current = beatmap.NewValue;

            if (!audioEquals || CurrentTrack.IsDummyDevice)
            {
                changeTrack();
            }
            else
            {
                // transfer still valid track to new working beatmap
                current.TransferTrack(beatmap.OldValue.Track);
            }

            TrackChanged?.Invoke(current, direction);

            ResetTrackAdjustments();

            queuedDirection = null;
        }

        private void changeTrack()
        {
            var lastTrack = CurrentTrack;

            var newTrack = new DrawableTrack(current.LoadTrack());
            newTrack.Completed += () => onTrackCompleted(current);

            CurrentTrack = newTrack;

            // At this point we may potentially be in an async context from tests. This is extremely dangerous but we have to make do for now.
            // CurrentTrack is immediately updated above for situations where a immediate knowledge about the new track is required,
            // but the mutation of the hierarchy is scheduled to avoid exceptions.
            Schedule(() =>
            {
                lastTrack.Expire();

                if (newTrack == CurrentTrack)
                    AddInternal(newTrack);
                else
                {
                    // If the track has changed via changeTrack() being called multiple times in a single update, force disposal on the old track.
                    newTrack.Dispose();
                }
            });
        }

        private void onTrackCompleted(WorkingBeatmap workingBeatmap)
        {
            // the source of track completion is the audio thread, so the beatmap may have changed before firing.
            if (current != workingBeatmap)
                return;

            if (!CurrentTrack.Looping && !beatmap.Disabled)
                NextTrack();
        }

        private bool allowRateAdjustments;

        /// <summary>
        /// Whether mod rate adjustments are allowed to be applied.
        /// </summary>
        public bool AllowRateAdjustments
        {
            get => allowRateAdjustments;
            set
            {
                if (allowRateAdjustments == value)
                    return;

                allowRateAdjustments = value;
                ResetTrackAdjustments();
            }
        }

        public void ResetTrackAdjustments()
        {
            CurrentTrack.ResetSpeedAdjustments();

            if (allowRateAdjustments)
            {
                foreach (var mod in mods.Value.OfType<IApplicableToTrack>())
                    mod.ApplyToTrack(CurrentTrack);
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
                    if (next())
                        onScreenDisplay?.Display(new MusicControllerToast("Next track"));

                    return true;

                case GlobalAction.MusicPrev:
                    switch (prev())
                    {
                        case PreviousTrackResult.Restart:
                            onScreenDisplay?.Display(new MusicControllerToast("Restart track"));
                            break;

                        case PreviousTrackResult.Previous:
                            onScreenDisplay?.Display(new MusicControllerToast("Previous track"));
                            break;
                    }

                    return true;
            }

            return false;
        }

        public void OnReleased(GlobalAction action)
        {
        }

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

    public enum PreviousTrackResult
    {
        None,
        Restart,
        Previous
    }
}
