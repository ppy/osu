// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// Encapsulates gameplay timing logic and provides a <see cref="GameplayClock"/> via DI for gameplay components to use.
    /// </summary>
    [Cached]
    public abstract class GameplayClockContainer : Container, IAdjustableClock
    {
        /// <summary>
        /// The final clock which is exposed to gameplay components.
        /// </summary>
        public GameplayClock GameplayClock { get; private set; }

        /// <summary>
        /// Whether gameplay is paused.
        /// </summary>
        public readonly BindableBool IsPaused = new BindableBool();

        /// <summary>
        /// The adjustable source clock used for gameplay. Should be used for seeks and clock control.
        /// </summary>
        protected readonly DecoupleableInterpolatingFramedClock AdjustableSource;

        /// <summary>
        /// The source clock.
        /// </summary>
        protected IClock SourceClock { get; private set; }

        /// <summary>
        /// Invoked when a seek has been performed via <see cref="Seek"/>
        /// </summary>
        public event Action OnSeek;

        /// <summary>
        /// Creates a new <see cref="GameplayClockContainer"/>.
        /// </summary>
        /// <param name="sourceClock">The source <see cref="IClock"/> used for timing.</param>
        protected GameplayClockContainer(IClock sourceClock)
        {
            SourceClock = sourceClock;

            RelativeSizeAxes = Axes.Both;

            AdjustableSource = new DecoupleableInterpolatingFramedClock { IsCoupled = false };
            IsPaused.BindValueChanged(OnIsPausedChanged);
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            dependencies.CacheAs(GameplayClock = CreateGameplayClock(AdjustableSource));
            GameplayClock.IsPaused.BindTo(IsPaused);

            return dependencies;
        }

        /// <summary>
        /// Starts gameplay.
        /// </summary>
        public virtual void Start()
        {
            ensureSourceClockSet();

            if (!AdjustableSource.IsRunning)
            {
                // Seeking the decoupled clock to its current time ensures that its source clock will be seeked to the same time
                // This accounts for the clock source potentially taking time to enter a completely stopped state
                Seek(GameplayClock.CurrentTime);

                AdjustableSource.Start();
            }

            IsPaused.Value = false;
        }

        /// <summary>
        /// Seek to a specific time in gameplay.
        /// </summary>
        /// <param name="time">The destination time to seek to.</param>
        public virtual void Seek(double time)
        {
            AdjustableSource.Seek(time);

            // Manually process to make sure the gameplay clock is correctly updated after a seek.
            GameplayClock.UnderlyingClock.ProcessFrame();

            OnSeek?.Invoke();
        }

        /// <summary>
        /// Stops gameplay.
        /// </summary>
        public void Stop() => IsPaused.Value = true;

        /// <summary>
        /// Resets this <see cref="GameplayClockContainer"/> and the source to an initial state ready for gameplay.
        /// </summary>
        public virtual void Reset()
        {
            ensureSourceClockSet();
            Seek(0);

            // Manually stop the source in order to not affect the IsPaused state.
            AdjustableSource.Stop();

            if (!IsPaused.Value)
                Start();
        }

        /// <summary>
        /// Changes the source clock.
        /// </summary>
        /// <param name="sourceClock">The new source.</param>
        protected void ChangeSource(IClock sourceClock) => AdjustableSource.ChangeSource(SourceClock = sourceClock);

        /// <summary>
        /// Ensures that the <see cref="AdjustableSource"/> is set to <see cref="SourceClock"/>, if it hasn't been given a source yet.
        /// This is usually done before a seek to avoid accidentally seeking only the adjustable source in decoupled mode,
        /// but not the actual source clock.
        /// That will pretty much only happen on the very first call of this method, as the source clock is passed in the constructor,
        /// but it is not yet set on the adjustable source there.
        /// </summary>
        private void ensureSourceClockSet()
        {
            if (AdjustableSource.Source == null)
                ChangeSource(SourceClock);
        }

        protected override void Update()
        {
            if (!IsPaused.Value)
                GameplayClock.UnderlyingClock.ProcessFrame();

            base.Update();
        }

        /// <summary>
        /// Invoked when the value of <see cref="IsPaused"/> is changed to start or stop the <see cref="AdjustableSource"/> clock.
        /// </summary>
        /// <param name="isPaused">Whether the clock should now be paused.</param>
        protected virtual void OnIsPausedChanged(ValueChangedEvent<bool> isPaused)
        {
            if (isPaused.NewValue)
                AdjustableSource.Stop();
            else
                AdjustableSource.Start();
        }

        /// <summary>
        /// Creates the final <see cref="GameplayClock"/> which is exposed via DI to be used by gameplay components.
        /// </summary>
        /// <remarks>
        /// Any intermediate clocks such as platform offsets should be applied here.
        /// </remarks>
        /// <param name="source">The <see cref="IFrameBasedClock"/> providing the source time.</param>
        /// <returns>The final <see cref="GameplayClock"/>.</returns>
        protected abstract GameplayClock CreateGameplayClock(IFrameBasedClock source);

        #region IAdjustableClock

        bool IAdjustableClock.Seek(double position)
        {
            Seek(position);
            return true;
        }

        void IAdjustableClock.Reset() => Reset();

        public void ResetSpeedAdjustments()
        {
        }

        double IAdjustableClock.Rate
        {
            get => GameplayClock.Rate;
            set => throw new NotSupportedException();
        }

        double IClock.Rate => GameplayClock.Rate;

        public double CurrentTime => GameplayClock.CurrentTime;

        public bool IsRunning => GameplayClock.IsRunning;

        #endregion
    }
}
