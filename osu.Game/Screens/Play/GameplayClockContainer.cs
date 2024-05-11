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
    [Cached(typeof(GameplayClockContainer))]
    public partial class GameplayClockContainer : Container, IAdjustableClock, IGameplayClock
    {
        public IBindable<bool> IsPaused => isPaused;

        public bool IsRewinding => GameplayClock.IsRewinding;

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
        /// <param name="requireDecoupling">Whether decoupling logic should be applied on the source clock.</param>
        public GameplayClockContainer(IClock sourceClock, bool applyOffsets, bool requireDecoupling)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                GameplayClock = new FramedBeatmapClock(applyOffsets, requireDecoupling, sourceClock),
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

        protected virtual void StartGameplayClock()
        {
            Logger.Log($"{nameof(GameplayClockContainer)} started via call to {nameof(StartGameplayClock)}");
            GameplayClock.Start();
        }

        protected virtual void StopGameplayClock()
        {
            Logger.Log($"{nameof(GameplayClockContainer)} stopped via call to {nameof(StopGameplayClock)}");
            GameplayClock.Stop();
        }

        /// <summary>
        /// Resets this <see cref="GameplayClockContainer"/> and the source to an initial state ready for gameplay.
        /// </summary>
        /// <param name="time">The time to seek to on resetting. If <c>null</c>, the existing <see cref="StartTime"/> will be used.</param>
        /// <param name="startClock">Whether to start the clock immediately. If <c>false</c> and the clock was already paused, the clock will remain paused after this call.
        /// </param>
        public void Reset(double? time = null, bool startClock = false)
        {
            bool wasPaused = isPaused.Value;

            // The intention of the Reset method is to get things into a known sane state.
            // As such, we intentionally stop the underlying clock directly here, bypassing Stop/StopGameplayClock.
            // This is to avoid any kind of isPaused state checks and frequency ramping (as provided by MasterGameplayClockContainer).
            GameplayClock.Stop();

            if (time != null)
                StartTime = time.Value;

            Seek(StartTime);

            if (!wasPaused || startClock)
                Start();
        }

        /// <summary>
        /// Changes the source clock.
        /// </summary>
        /// <param name="sourceClock">The new source.</param>
        protected void ChangeSource(IClock sourceClock) => GameplayClock.ChangeSource(sourceClock);

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
    }
}
