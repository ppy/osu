// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays
{
    /// <summary>
    /// Handles playback of the global music track.
    /// </summary>
    public class MusicController : CompositeDrawable
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

        /// <summary>
        /// Whether the user has requested the track to be paused. Use <see cref="IsPlaying"/> to determine whether the track is still playing.
        /// </summary>
        public bool UserPauseRequested { get; private set; }

        /// <summary>
        /// Fired when the global <see cref="WorkingBeatmap"/> has changed.
        /// Includes direction information for display purposes.
        /// </summary>
        public event Action<WorkingBeatmap, TrackChangeDirection> TrackChanged;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; }

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; }

        [NotNull]
        public DrawableTrack CurrentTrack { get; private set; } = new DrawableTrack(new TrackVirtual(1000));

        [BackgroundDependencyLoader]
        private void load()
        {
            beatmaps.ItemUpdated += beatmapUpdated;
            beatmaps.ItemRemoved += beatmapRemoved;

            beatmapSets.AddRange(beatmaps.GetAllUsableBeatmapSets(IncludedDetails.Minimal, true).OrderBy(_ => RNG.Next()));

            // Todo: These binds really shouldn't be here, but are unlikely to cause any issues for now.
            // They are placed here for now since some tests rely on setting the beatmap _and_ their hierarchies inside their load(), which runs before the MusicController's load().
            beatmap.BindValueChanged(beatmapChanged, true);
            mods.BindValueChanged(_ => ResetTrackAdjustments(), true);
        }

        /// <summary>
        /// Forcefully reload the current <see cref="WorkingBeatmap"/>'s track from disk.
        /// </summary>
        public void ReloadCurrentTrack() => changeTrack();

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

        private void beatmapUpdated(BeatmapSetInfo set) => Schedule(() =>
        {
            beatmapSets.Remove(set);
            beatmapSets.Add(set);
        });

        private void beatmapRemoved(BeatmapSetInfo set) => Schedule(() => beatmapSets.RemoveAll(s => s.ID == set.ID));

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
            if (UserPauseRequested) return;

            if (CurrentTrack.IsDummyDevice || beatmap.Value.BeatmapSetInfo.DeletePending)
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
        /// <param name="restart">Whether to restart the track from the beginning.</param>
        /// <param name="requestedByUser">
        /// Whether the request to play was issued by the user rather than internally.
        /// Specifying <c>true</c> will ensure that other methods like <see cref="EnsurePlayingSomething"/>
        /// will resume music playback going forward.
        /// </param>
        /// <returns>Whether the operation was successful.</returns>
        public bool Play(bool restart = false, bool requestedByUser = false)
        {
            if (requestedByUser)
                UserPauseRequested = false;

            if (restart)
                CurrentTrack.Restart();
            else if (!IsPlaying)
                CurrentTrack.Start();

            return true;
        }

        /// <summary>
        /// Stop playing the current track and pause at the current position.
        /// </summary>
        /// <param name="requestedByUser">
        /// Whether the request to stop was issued by the user rather than internally.
        /// Specifying <c>true</c> will ensure that other methods like <see cref="EnsurePlayingSomething"/>
        /// will not resume music playback until the next explicit call to <see cref="Play"/>.
        /// </param>
        public void Stop(bool requestedByUser = false)
        {
            UserPauseRequested |= requestedByUser;
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
                Stop(true);
            else
                Play(requestedByUser: true);

            return true;
        }

        /// <summary>
        /// Play the previous track or restart the current track if it's current time below <see cref="restart_cutoff_point"/>.
        /// </summary>
        /// <param name="onSuccess">Invoked when the operation has been performed successfully.</param>
        public void PreviousTrack(Action<PreviousTrackResult> onSuccess = null) => Schedule(() =>
        {
            PreviousTrackResult res = prev();
            if (res != PreviousTrackResult.None)
                onSuccess?.Invoke(res);
        });

        /// <summary>
        /// Play the previous track or restart the current track if it's current time below <see cref="restart_cutoff_point"/>.
        /// </summary>
        /// <returns>The <see cref="PreviousTrackResult"/> that indicate the decided action.</returns>
        private PreviousTrackResult prev()
        {
            if (beatmap.Disabled)
                return PreviousTrackResult.None;

            double currentTrackPosition = CurrentTrack.CurrentTime;

            if (currentTrackPosition >= restart_cutoff_point)
            {
                SeekTo(0);
                return PreviousTrackResult.Restart;
            }

            queuedDirection = TrackChangeDirection.Prev;

            var playable = BeatmapSets.TakeWhile(i => i.ID != current.BeatmapSetInfo.ID).LastOrDefault() ?? BeatmapSets.LastOrDefault();

            if (playable != null)
            {
                changeBeatmap(beatmaps.GetWorkingBeatmap(playable.Beatmaps.First()));
                restartTrack();
                return PreviousTrackResult.Previous;
            }

            return PreviousTrackResult.None;
        }

        /// <summary>
        /// Play the next random or playlist track.
        /// </summary>
        /// <param name="onSuccess">Invoked when the operation has been performed successfully.</param>
        /// <returns>A <see cref="ScheduledDelegate"/> of the operation.</returns>
        public void NextTrack(Action onSuccess = null) => Schedule(() =>
        {
            bool res = next();
            if (res)
                onSuccess?.Invoke();
        });

        private bool next()
        {
            if (beatmap.Disabled)
                return false;

            queuedDirection = TrackChangeDirection.Next;

            var playable = BeatmapSets.SkipWhile(i => i.ID != current.BeatmapSetInfo.ID).ElementAtOrDefault(1) ?? BeatmapSets.FirstOrDefault();

            if (playable != null)
            {
                changeBeatmap(beatmaps.GetWorkingBeatmap(playable.Beatmaps.First()));
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

        private void beatmapChanged(ValueChangedEvent<WorkingBeatmap> beatmap) => changeBeatmap(beatmap.NewValue);

        private void changeBeatmap(WorkingBeatmap newWorking)
        {
            // This method can potentially be triggered multiple times as it is eagerly fired in next() / prev() to ensure correct execution order
            // (changeBeatmap must be called before consumers receive the bindable changed event, which is not the case when the local beatmap bindable is updated directly).
            if (newWorking == current)
                return;

            var lastWorking = current;

            TrackChangeDirection direction = TrackChangeDirection.None;

            bool audioEquals = newWorking?.BeatmapInfo?.AudioEquals(current?.BeatmapInfo) ?? false;

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
                    int last = BeatmapSets.TakeWhile(b => b.ID != current.BeatmapSetInfo?.ID).Count();
                    int next = newWorking == null ? -1 : BeatmapSets.TakeWhile(b => b.ID != newWorking.BeatmapSetInfo?.ID).Count();

                    direction = last > next ? TrackChangeDirection.Prev : TrackChangeDirection.Next;
                }
            }

            current = newWorking;

            if (!audioEquals || CurrentTrack.IsDummyDevice)
            {
                changeTrack();
            }
            else
            {
                // transfer still valid track to new working beatmap
                current.TransferTrack(lastWorking.Track);
            }

            TrackChanged?.Invoke(current, direction);

            ResetTrackAdjustments();

            queuedDirection = null;

            // this will be a noop if coming from the beatmapChanged event.
            // the exception is local operations like next/prev, where we want to complete loading the track before sending out a change.
            if (beatmap.Value != current && beatmap is Bindable<WorkingBeatmap> working)
                working.Value = current;
        }

        private void changeTrack()
        {
            var lastTrack = CurrentTrack;

            var queuedTrack = new DrawableTrack(current.LoadTrack());
            queuedTrack.Completed += () => onTrackCompleted(current);

            CurrentTrack = queuedTrack;

            // At this point we may potentially be in an async context from tests. This is extremely dangerous but we have to make do for now.
            // CurrentTrack is immediately updated above for situations where a immediate knowledge about the new track is required,
            // but the mutation of the hierarchy is scheduled to avoid exceptions.
            Schedule(() =>
            {
                lastTrack.VolumeTo(0, 500, Easing.Out).Expire();

                if (queuedTrack == CurrentTrack)
                {
                    AddInternal(queuedTrack);
                    queuedTrack.VolumeTo(0).Then().VolumeTo(1, 300, Easing.Out);
                }
                else
                {
                    // If the track has changed since the call to changeTrack, it is safe to dispose the
                    // queued track rather than consume it.
                    queuedTrack.Dispose();
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

        private bool allowTrackAdjustments;

        /// <summary>
        /// Whether mod track adjustments are allowed to be applied.
        /// </summary>
        public bool AllowTrackAdjustments
        {
            get => allowTrackAdjustments;
            set
            {
                if (allowTrackAdjustments == value)
                    return;

                allowTrackAdjustments = value;
                ResetTrackAdjustments();
            }
        }

        /// <summary>
        /// Resets the adjustments currently applied on <see cref="CurrentTrack"/> and applies the mod adjustments if <see cref="AllowTrackAdjustments"/> is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// Does not reset any adjustments applied directly to the beatmap track.
        /// </remarks>
        public void ResetTrackAdjustments()
        {
            CurrentTrack.RemoveAllAdjustments(AdjustableProperty.Balance);
            CurrentTrack.RemoveAllAdjustments(AdjustableProperty.Frequency);
            CurrentTrack.RemoveAllAdjustments(AdjustableProperty.Tempo);
            CurrentTrack.RemoveAllAdjustments(AdjustableProperty.Volume);

            if (allowTrackAdjustments)
            {
                foreach (var mod in mods.Value.OfType<IApplicableToTrack>())
                    mod.ApplyToTrack(CurrentTrack);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (beatmaps != null)
            {
                beatmaps.ItemUpdated -= beatmapUpdated;
                beatmaps.ItemRemoved -= beatmapRemoved;
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
