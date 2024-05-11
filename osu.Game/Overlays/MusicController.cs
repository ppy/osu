// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays
{
    /// <summary>
    /// Handles playback of the global music track.
    /// </summary>
    public partial class MusicController : CompositeDrawable
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        /// <summary>
        /// Point in time after which the current track will be restarted on triggering a "previous track" action.
        /// </summary>
        private const double restart_cutoff_point = 5000;

        /// <summary>
        /// Whether the user has requested the track to be paused. Use <see cref="IsPlaying"/> to determine whether the track is still playing.
        /// </summary>
        public bool UserPauseRequested { get; private set; }

        /// <summary>
        /// Whether user control of the global track should be allowed.
        /// </summary>
        public readonly BindableBool AllowTrackControl = new BindableBool(true);

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

        [Resolved]
        private RealmAccess realm { get; set; }

        [Resolved]
        private AudioManager audioManager { get; set; }

        [Resolved]
        private OsuGameBase gameBase { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.BindValueChanged(b => changeBeatmap(b.NewValue), true);
            mods.BindValueChanged(_ => ResetTrackAdjustments(), true);
        }

        /// <summary>
        /// Forcefully reload the current <see cref="WorkingBeatmap"/>'s track from disk.
        /// </summary>
        public void ReloadCurrentTrack()
        {
            changeTrack();
            TrackChanged?.Invoke(current, TrackChangeDirection.None);
        }

        /// <summary>
        /// Returns whether the beatmap track is playing.
        /// </summary>
        public bool IsPlaying => CurrentTrack.IsRunning;

        /// <summary>
        /// Returns whether the beatmap track is loaded.
        /// </summary>
        public bool TrackLoaded => CurrentTrack.TrackLoaded;

        private ScheduledDelegate seekDelegate;

        public void SeekTo(double position)
        {
            seekDelegate?.Cancel();
            seekDelegate = Schedule(() =>
            {
                if (beatmap.Disabled || !AllowTrackControl.Value)
                    return;

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
                if (beatmap.Disabled || !AllowTrackControl.Value)
                    return;

                Logger.Log($"{nameof(MusicController)} skipping next track to {nameof(EnsurePlayingSomething)}");
                NextTrack();
            }
            else if (!IsPlaying)
            {
                Logger.Log($"{nameof(MusicController)} starting playback to {nameof(EnsurePlayingSomething)}");
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
            if (requestedByUser && !AllowTrackControl.Value)
                return false;

            if (requestedByUser)
                UserPauseRequested = false;

            if (restart)
                CurrentTrack.RestartAsync();
            else if (!IsPlaying)
                CurrentTrack.StartAsync();

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
            if (requestedByUser && !AllowTrackControl.Value)
                return;

            UserPauseRequested |= requestedByUser;
            if (CurrentTrack.IsRunning)
                CurrentTrack.StopAsync();
        }

        /// <summary>
        /// Toggle pause / play.
        /// </summary>
        /// <returns>Whether the operation was successful.</returns>
        public bool TogglePause()
        {
            if (!AllowTrackControl.Value)
                return false;

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
            if (beatmap.Disabled || !AllowTrackControl.Value)
                return PreviousTrackResult.None;

            double currentTrackPosition = CurrentTrack.CurrentTime;

            if (currentTrackPosition >= restart_cutoff_point)
            {
                SeekTo(0);
                return PreviousTrackResult.Restart;
            }

            queuedDirection = TrackChangeDirection.Prev;

            var playableSet = getBeatmapSets().AsEnumerable().TakeWhile(i => !i.Equals(current.BeatmapSetInfo)).LastOrDefault()
                              ?? getBeatmapSets().LastOrDefault();

            if (playableSet != null)
            {
                changeBeatmap(beatmaps.GetWorkingBeatmap(playableSet.Beatmaps.First()));
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
            if (beatmap.Disabled || !AllowTrackControl.Value)
                return false;

            queuedDirection = TrackChangeDirection.Next;

            var playableSet = getBeatmapSets().AsEnumerable().SkipWhile(i => !i.Equals(current.BeatmapSetInfo)).ElementAtOrDefault(1)
                              ?? getBeatmapSets().FirstOrDefault();

            var playableBeatmap = playableSet?.Beatmaps.FirstOrDefault();

            if (playableBeatmap != null)
            {
                changeBeatmap(beatmaps.GetWorkingBeatmap(playableBeatmap));
                restartTrack();
                return true;
            }

            return false;
        }

        private void restartTrack()
        {
            // if not scheduled, the previously track will be stopped one frame later (see ScheduleAfterChildren logic in GameBase).
            // we probably want to move this to a central method for switching to a new working beatmap in the future.
            Schedule(() => CurrentTrack.RestartAsync());
        }

        private WorkingBeatmap current;

        private TrackChangeDirection? queuedDirection;

        private IQueryable<BeatmapSetInfo> getBeatmapSets() => realm.Realm.All<BeatmapSetInfo>().Where(s => !s.DeletePending);

        private void changeBeatmap(WorkingBeatmap newWorking)
        {
            // This method can potentially be triggered multiple times as it is eagerly fired in next() / prev() to ensure correct execution order
            // (changeBeatmap must be called before consumers receive the bindable changed event, which is not the case when the local beatmap bindable is updated directly).
            if (newWorking == current)
                return;

            var lastWorking = current;

            TrackChangeDirection direction = TrackChangeDirection.None;

            bool audioEquals = newWorking?.BeatmapInfo?.AudioEquals(current?.BeatmapInfo) == true;

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
                    int last = getBeatmapSets().AsEnumerable().TakeWhile(b => !b.Equals(current.BeatmapSetInfo)).Count();
                    int next = newWorking == null ? -1 : getBeatmapSets().AsEnumerable().TakeWhile(b => !b.Equals(newWorking.BeatmapSetInfo)).Count();

                    direction = last > next ? TrackChangeDirection.Prev : TrackChangeDirection.Next;
                }
            }

            current = newWorking;

            if (lastWorking == null || !lastWorking.TryTransferTrack(current))
                changeTrack();

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
            var queuedTrack = getQueuedTrack();

            var lastTrack = CurrentTrack;
            lastTrack.Completed -= onTrackCompleted;

            CurrentTrack = queuedTrack;

            gameBase.TrackNormalizeVolume.Value = current.BeatmapInfo.AudioNormalization?.IntegratedLoudnessInVolumeOffset ?? 0.8;

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

        private DrawableTrack getQueuedTrack()
        {
            // Important to keep this in its own method to avoid inadvertently capturing unnecessary variables in the callback.
            // Can lead to leaks.
            var queuedTrack = new DrawableTrack(current.LoadTrack());
            queuedTrack.Completed += onTrackCompleted;
            return queuedTrack;
        }

        private void onTrackCompleted()
        {
            if (!CurrentTrack.Looping && !beatmap.Disabled && AllowTrackControl.Value)
                NextTrack();
        }

        private bool applyModTrackAdjustments;

        /// <summary>
        /// Whether mod track adjustments are allowed to be applied.
        /// </summary>
        public bool ApplyModTrackAdjustments
        {
            get => applyModTrackAdjustments;
            set
            {
                if (applyModTrackAdjustments == value)
                    return;

                applyModTrackAdjustments = value;
                ResetTrackAdjustments();
            }
        }

        private AudioAdjustments modTrackAdjustments;

        /// <summary>
        /// Resets the adjustments currently applied on <see cref="CurrentTrack"/> and applies the mod adjustments if <see cref="ApplyModTrackAdjustments"/> is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// Does not reset any adjustments applied directly to the beatmap track.
        /// </remarks>
        public void ResetTrackAdjustments()
        {
            // todo: we probably want a helper method rather than this.
            CurrentTrack.RemoveAllAdjustments(AdjustableProperty.Balance);
            CurrentTrack.RemoveAllAdjustments(AdjustableProperty.Frequency);
            CurrentTrack.RemoveAllAdjustments(AdjustableProperty.Tempo);
            CurrentTrack.RemoveAllAdjustments(AdjustableProperty.Volume);

            if (applyModTrackAdjustments)
            {
                CurrentTrack.BindAdjustments(modTrackAdjustments = new AudioAdjustments());

                foreach (var mod in mods.Value.OfType<IApplicableToTrack>())
                    mod.ApplyToTrack(modTrackAdjustments);
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
