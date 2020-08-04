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
using osu.Framework.Input.Bindings;
using osu.Framework.Utils;
using osu.Framework.Threading;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Input.Bindings;
using osu.Game.Overlays.OSD;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays
{
    /// <summary>
    /// Handles playback of the global music track.
    /// </summary>
    public class MusicController : CompositeDrawable, IKeyBindingHandler<GlobalAction>, ITrack
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
        private readonly TrackContainer trackContainer;

        [CanBeNull]
        private DrawableTrack drawableTrack;

        [CanBeNull]
        private Track track;

        private IBindable<WeakReference<BeatmapSetInfo>> managerUpdated;
        private IBindable<WeakReference<BeatmapSetInfo>> managerRemoved;

        public MusicController()
        {
            InternalChild = trackContainer = new TrackContainer { RelativeSizeAxes = Axes.Both };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            managerUpdated = beatmaps.ItemUpdated.GetBoundCopy();
            managerUpdated.BindValueChanged(beatmapUpdated);
            managerRemoved = beatmaps.ItemRemoved.GetBoundCopy();
            managerRemoved.BindValueChanged(beatmapRemoved);

            beatmapSets.AddRange(beatmaps.GetAllUsableBeatmapSets(IncludedDetails.Minimal, true).OrderBy(_ => RNG.Next()));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

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
        public bool IsPlaying => drawableTrack?.IsRunning ?? false;

        /// <summary>
        /// Returns whether the beatmap track is loaded.
        /// </summary>
        public bool TrackLoaded => drawableTrack?.IsLoaded == true;

        /// <summary>
        /// Returns the current time of the beatmap track.
        /// </summary>
        public double CurrentTrackTime => drawableTrack?.CurrentTime ?? 0;

        /// <summary>
        /// Returns the length of the beatmap track.
        /// </summary>
        public double TrackLength => drawableTrack?.Length ?? 0;

        public void AddAdjustment(AdjustableProperty type, BindableNumber<double> adjustBindable)
            => trackContainer.AddAdjustment(type, adjustBindable);

        public void RemoveAdjustment(AdjustableProperty type, BindableNumber<double> adjustBindable)
            => trackContainer.RemoveAdjustment(type, adjustBindable);

        public void Reset() => drawableTrack?.Reset();

        [CanBeNull]
        public IAdjustableClock GetTrackClock() => track;

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
                    drawableTrack?.Seek(position);
            });
        }

        /// <summary>
        /// Ensures music is playing, no matter what, unless the user has explicitly paused.
        /// This means that if the current beatmap has a virtual track (see <see cref="TrackVirtual"/>) a new beatmap will be selected.
        /// </summary>
        public void EnsurePlayingSomething()
        {
            if (IsUserPaused) return;

            if (drawableTrack == null || drawableTrack.IsDummyDevice)
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

            if (drawableTrack == null)
                return false;

            if (restart)
                drawableTrack.Restart();
            else if (!IsPlaying)
                drawableTrack.Start();

            return true;
        }

        /// <summary>
        /// Stop playing the current track and pause at the current position.
        /// </summary>
        public void Stop()
        {
            IsUserPaused = true;
            if (drawableTrack?.IsRunning == true)
                drawableTrack.Stop();
        }

        /// <summary>
        /// Toggle pause / play.
        /// </summary>
        /// <returns>Whether the operation was successful.</returns>
        public bool TogglePause()
        {
            if (drawableTrack?.IsRunning == true)
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

            var currentTrackPosition = drawableTrack?.CurrentTime;

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
            Schedule(() => drawableTrack?.Restart());
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
                    // figure out the best direction based on order in playlist.
                    var last = BeatmapSets.TakeWhile(b => b.ID != current.BeatmapSetInfo?.ID).Count();
                    var next = beatmap.NewValue == null ? -1 : BeatmapSets.TakeWhile(b => b.ID != beatmap.NewValue.BeatmapSetInfo?.ID).Count();

                    direction = last > next ? TrackChangeDirection.Prev : TrackChangeDirection.Next;
                }
            }

            current = beatmap.NewValue;

            drawableTrack?.Expire();
            drawableTrack = null;
            track = null;

            if (current != null)
                trackContainer.Add(drawableTrack = new DrawableTrack(track = current.GetRealTrack()));

            TrackChanged?.Invoke(current, direction);

            ResetTrackAdjustments();

            queuedDirection = null;
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
            if (drawableTrack == null)
                return;

            drawableTrack.ResetSpeedAdjustments();

            if (allowRateAdjustments)
            {
                foreach (var mod in mods.Value.OfType<IApplicableToTrack>())
                    mod.ApplyToTrack(drawableTrack);
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

        private class TrackContainer : AudioContainer<DrawableTrack>
        {
        }

        #region ITrack

        /// <summary>
        /// The volume of this component.
        /// </summary>
        public BindableNumber<double> Volume => drawableTrack?.Volume; // Todo: Bad

        /// <summary>
        /// The playback balance of this sample (-1 .. 1 where 0 is centered)
        /// </summary>
        public BindableNumber<double> Balance => drawableTrack?.Balance; // Todo: Bad

        /// <summary>
        /// Rate at which the component is played back (affects pitch). 1 is 100% playback speed, or default frequency.
        /// </summary>
        public BindableNumber<double> Frequency => drawableTrack?.Frequency; // Todo: Bad

        /// <summary>
        /// Rate at which the component is played back (does not affect pitch). 1 is 100% playback speed.
        /// </summary>
        public BindableNumber<double> Tempo => drawableTrack?.Tempo; // Todo: Bad

        public IBindable<double> AggregateVolume => drawableTrack?.AggregateVolume; // Todo: Bad

        public IBindable<double> AggregateBalance => drawableTrack?.AggregateBalance; // Todo: Bad

        public IBindable<double> AggregateFrequency => drawableTrack?.AggregateFrequency; // Todo: Bad

        public IBindable<double> AggregateTempo => drawableTrack?.AggregateTempo; // Todo: Bad

        /// <summary>
        /// Overall playback rate (1 is 100%, -1 is reversed at 100%).
        /// </summary>
        public double Rate => AggregateFrequency.Value * AggregateTempo.Value;

        event Action ITrack.Completed
        {
            add
            {
                if (drawableTrack != null)
                    drawableTrack.Completed += value;
            }
            remove
            {
                if (drawableTrack != null)
                    drawableTrack.Completed -= value;
            }
        }

        event Action ITrack.Failed
        {
            add
            {
                if (drawableTrack != null)
                    drawableTrack.Failed += value;
            }
            remove
            {
                if (drawableTrack != null)
                    drawableTrack.Failed -= value;
            }
        }

        public bool Looping
        {
            get => drawableTrack?.Looping ?? false;
            set
            {
                if (drawableTrack != null)
                    drawableTrack.Looping = value;
            }
        }

        public bool IsDummyDevice => drawableTrack?.IsDummyDevice ?? true;

        public double RestartPoint
        {
            get => drawableTrack?.RestartPoint ?? 0;
            set
            {
                if (drawableTrack != null)
                    drawableTrack.RestartPoint = value;
            }
        }

        double ITrack.CurrentTime => CurrentTrackTime;

        double ITrack.Length
        {
            get => TrackLength;
            set
            {
                if (drawableTrack != null)
                    drawableTrack.Length = value;
            }
        }

        public int? Bitrate => drawableTrack?.Bitrate;

        bool ITrack.IsRunning => IsPlaying;

        public bool IsReversed => drawableTrack?.IsReversed ?? false;

        public bool HasCompleted => drawableTrack?.HasCompleted ?? false;

        void ITrack.Reset() => drawableTrack?.Reset();

        void ITrack.Restart() => Play(true);

        void ITrack.ResetSpeedAdjustments() => ResetTrackAdjustments();

        bool ITrack.Seek(double seek)
        {
            SeekTo(seek);
            return true;
        }

        void ITrack.Start() => Play();

        public ChannelAmplitudes CurrentAmplitudes => drawableTrack?.CurrentAmplitudes ?? ChannelAmplitudes.Empty;

        #endregion
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
