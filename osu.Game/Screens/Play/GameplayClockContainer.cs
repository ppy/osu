// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Timing;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// Encapsulates gameplay timing logic and provides a <see cref="IGameplayClock"/> via DI for gameplay components to use.
    /// </summary>
    [Cached(typeof(IGameplayClock))]
    public partial class GameplayClockContainer : Container, IAdjustableClock, IGameplayClock
    {
        public IBindable<bool> IsPaused => isPaused;

        public bool IsRewinding => GameplayClock.IsRewinding;

        /// <summary>
        /// The source clock. Should generally not be used for any timekeeping purposes.
        /// </summary>
        public IClock SourceClock { get; private set; }

        /// <summary>
        /// Invoked when a seek has been performed via <see cref="Seek"/>
        /// </summary>
        public event Action? OnSeek;

        /// <summary>
        /// The time from which the clock should start. Will be seeked to on calling <see cref="Reset"/>.
        /// Can be adjusted by calling <see cref="Reset"/> with a time value.
        /// </summary>
        /// <remarks>
        /// By default, a value of zero will be used.
        /// Importantly, the value will be inferred from the current beatmap in <see cref="MasterGameplayClockContainer"/> by default.
        /// </remarks>
        public double StartTime { get; protected set; }

        public IAdjustableAudioComponent AdjustmentsFromMods { get; } = new AudioAdjustments();

        private readonly BindableBool isPaused = new BindableBool(true);

        /// <summary>
        /// The adjustable source clock used for gameplay. Should be used for seeks and clock control.
        /// This is the final source exposed to gameplay components <see cref="IGameplayClock"/> via delegation in this class.
        /// </summary>
        protected readonly FramedBeatmapClock GameplayClock;

        protected override Container<Drawable> Content { get; } = new Container { RelativeSizeAxes = Axes.Both };

        /// <summary>
        /// Creates a new <see cref="GameplayClockContainer"/>.
        /// </summary>
        /// <param name="sourceClock">The source <see cref="IClock"/> used for timing.</param>
        /// <param name="applyOffsets">Whether to apply platform, user and beatmap offsets to the mix.</param>
        public GameplayClockContainer(IClock sourceClock, bool applyOffsets = false)
        {
            SourceClock = sourceClock;

            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                GameplayClock = new FramedBeatmapClock(applyOffsets) { IsCoupled = false },
                Content
            };
        }

        /// <summary>
        /// Starts gameplay and marks un-paused state.
        /// </summary>
        public void Start()
        {
            if (!isPaused.Value)
                return;

            isPaused.Value = false;

            ensureSourceClockSet();

            PrepareStart();

            // The case which caused this to be added is FrameStabilityContainer, which manages its own current and elapsed time.
            // Because we generally update our own current time quicker than children can query it (via Start/Seek/Update),
            // this means that the first frame ever exposed to children may have a non-zero current time.
            //
            // If the child component is not aware of the parent ElapsedFrameTime (which is the case for FrameStabilityContainer)
            // they will take on the new CurrentTime with a zero elapsed time. This can in turn cause components to behave incorrectly
            // if they are intending to trigger events at the precise StartTime (ie. DrawableStoryboardSample).
            //
            // By scheduling the start call, children are guaranteed to receive one frame at the original start time, allowing
            // then to progress with a correct locally calculated elapsed time.
            SchedulerAfterChildren.Add(() =>
            {
                if (isPaused.Value)
                    return;

                StartGameplayClock();
            });
        }

        /// <summary>
        /// When <see cref="Start"/> is called, this will be run to give an opportunity to prepare the clock at the correct
        /// start location.
        /// </summary>
        protected virtual void PrepareStart()
        {
        }

        /// <summary>
        /// Seek to a specific time in gameplay.
        /// </summary>
        /// <param name="time">The destination time to seek to.</param>
        public virtual void Seek(double time)
        {
            Logger.Log($"{nameof(GameplayClockContainer)} seeking to {time}");

            GameplayClock.Seek(time);

            OnSeek?.Invoke();
        }

        /// <summary>
        /// Stops gameplay and marks paused state.
        /// </summary>
        public void Stop()
        {
            if (isPaused.Value)
                return;

            isPaused.Value = true;
            StopGameplayClock();
        }

        protected virtual void StartGameplayClock() => GameplayClock.Start();
        protected virtual void StopGameplayClock() => GameplayClock.Stop();

        /// <summary>
        /// Resets this <see cref="GameplayClockContainer"/> and the source to an initial state ready for gameplay.
        /// </summary>
        /// <param name="time">The time to seek to on resetting. If <c>null</c>, the existing <see cref="StartTime"/> will be used.</param>
        /// <param name="startClock">Whether to start the clock immediately, if not already started.</param>
        public void Reset(double? time = null, bool startClock = false)
        {
            bool wasPaused = isPaused.Value;

            Stop();

            ensureSourceClockSet();

            if (time != null)
                StartTime = time.Value;

            Seek(StartTime);

            // This is a workaround for the fact that DecoupleableInterpolatingFramedClock doesn't seek the source
            // if the source is not IsRunning. (see https://github.com/ppy/osu-framework/blob/2102638056dfcf85d21b4d85266d53b5dd018767/osu.Framework/Timing/DecoupleableInterpolatingFramedClock.cs#L209-L210)
            // I hope to remove this once we knock some sense into clocks in general.
            //
            // Without this seek, the multiplayer spectator start sequence breaks:
            // - Individual clients' clocks are never updated to their expected time
            // - The sync manager thinks they are running behind
            // - Gameplay doesn't start when it should (until a timeout occurs because nothing is happening for 10+ seconds)
            //
            // In addition, we use `CurrentTime` for this seek instead of `StartTime` as the above seek may have applied inherent
            // offsets which need to be accounted for (ie. FramedBeatmapClock.TotalAppliedOffset).
            //
            // See https://github.com/ppy/osu/pull/24451/files/87fee001c786b29db34063ef3350e9a9f024d3ab#diff-28ca02979641e2d98a15fe5d5e806f56acf60ac100258a059fa72503b6cc54e8.
            (SourceClock as IAdjustableClock)?.Seek(CurrentTime);

            if (!wasPaused || startClock)
                Start();
        }

        /// <summary>
        /// Changes the source clock.
        /// </summary>
        /// <param name="sourceClock">The new source.</param>
        protected void ChangeSource(IClock sourceClock) => GameplayClock.ChangeSource(SourceClock = sourceClock);

        /// <summary>
        /// Ensures that the <see cref="GameplayClock"/> is set to <see cref="SourceClock"/>, if it hasn't been given a source yet.
        /// This is usually done before a seek to avoid accidentally seeking only the adjustable source in decoupled mode,
        /// but not the actual source clock.
        /// That will pretty much only happen on the very first call of this method, as the source clock is passed in the constructor,
        /// but it is not yet set on the adjustable source there.
        /// </summary>
        private void ensureSourceClockSet()
        {
            if (GameplayClock.Source == null)
                ChangeSource(SourceClock);
        }

        #region IAdjustableClock

        bool IAdjustableClock.Seek(double position)
        {
            Seek(position);
            return true;
        }

        void IAdjustableClock.Reset() => Reset();

        public virtual void ResetSpeedAdjustments()
        {
        }

        double IAdjustableClock.Rate
        {
            get => GameplayClock.Rate;
            set => throw new NotSupportedException();
        }

        public double Rate => GameplayClock.Rate;

        public double CurrentTime => GameplayClock.CurrentTime;

        public bool IsRunning => GameplayClock.IsRunning;

        #endregion

        public void ProcessFrame()
        {
            // Handled via update. Don't process here to safeguard from external usages potentially processing frames additional times.
        }

        public double ElapsedFrameTime => GameplayClock.ElapsedFrameTime;

        public double FramesPerSecond => GameplayClock.FramesPerSecond;

        public FrameTimeInfo TimeInfo => GameplayClock.TimeInfo;
    }
}
