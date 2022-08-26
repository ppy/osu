// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Timing;
using osu.Framework.Utils;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// Encapsulates gameplay timing logic and provides a <see cref="IGameplayClock"/> via DI for gameplay components to use.
    /// </summary>
    public class GameplayClockContainer : Container, IAdjustableClock, IGameplayClock
    {
        /// <summary>
        /// Whether gameplay is paused.
        /// </summary>
        public IBindable<bool> IsPaused => isPaused;

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
        /// </summary>
        /// <remarks>
        /// If not set, a value of zero will be used.
        /// Importantly, the value will be inferred from the current ruleset in <see cref="MasterGameplayClockContainer"/> unless specified.
        /// </remarks>
        public double? StartTime { get; set; }

        public virtual IEnumerable<double> NonGameplayAdjustments => Enumerable.Empty<double>();

        /// <summary>
        /// The final clock which is exposed to gameplay components.
        /// </summary>
        protected IFrameBasedClock FramedClock { get; private set; }

        private readonly BindableBool isPaused = new BindableBool(true);

        /// <summary>
        /// The adjustable source clock used for gameplay. Should be used for seeks and clock control.
        /// </summary>
        private readonly DecoupleableInterpolatingFramedClock decoupledClock;

        /// <summary>
        /// Creates a new <see cref="GameplayClockContainer"/>.
        /// </summary>
        /// <param name="sourceClock">The source <see cref="IClock"/> used for timing.</param>
        public GameplayClockContainer(IClock sourceClock)
        {
            SourceClock = sourceClock;

            RelativeSizeAxes = Axes.Both;

            decoupledClock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };
            IsPaused.BindValueChanged(OnIsPausedChanged);

            // this will be replaced during load, but non-null for tests which don't add this component to the hierarchy.
            FramedClock = new FramedClock();
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            FramedClock = CreateGameplayClock(decoupledClock);

            dependencies.CacheAs<IGameplayClock>(this);

            return dependencies;
        }

        /// <summary>
        /// Starts gameplay.
        /// </summary>
        public virtual void Start()
        {
            ensureSourceClockSet();

            if (!decoupledClock.IsRunning)
            {
                // Seeking the decoupled clock to its current time ensures that its source clock will be seeked to the same time
                // This accounts for the clock source potentially taking time to enter a completely stopped state
                Seek(FramedClock.CurrentTime);

                decoupledClock.Start();
            }

            isPaused.Value = false;
        }

        /// <summary>
        /// Seek to a specific time in gameplay.
        /// </summary>
        /// <param name="time">The destination time to seek to.</param>
        public virtual void Seek(double time)
        {
            Logger.Log($"{nameof(GameplayClockContainer)} seeking to {time}");

            decoupledClock.Seek(time);

            // Manually process to make sure the gameplay clock is correctly updated after a seek.
            FramedClock.ProcessFrame();

            OnSeek?.Invoke();
        }

        /// <summary>
        /// Stops gameplay.
        /// </summary>
        public void Stop() => isPaused.Value = true;

        /// <summary>
        /// Resets this <see cref="GameplayClockContainer"/> and the source to an initial state ready for gameplay.
        /// </summary>
        /// <param name="startClock">Whether to start the clock immediately, if not already started.</param>
        public void Reset(bool startClock = false)
        {
            // Manually stop the source in order to not affect the IsPaused state.
            decoupledClock.Stop();

            if (!IsPaused.Value || startClock)
                Start();

            ensureSourceClockSet();
            Seek(StartTime ?? 0);
        }

        /// <summary>
        /// Changes the source clock.
        /// </summary>
        /// <param name="sourceClock">The new source.</param>
        protected void ChangeSource(IClock sourceClock) => decoupledClock.ChangeSource(SourceClock = sourceClock);

        /// <summary>
        /// Ensures that the <see cref="decoupledClock"/> is set to <see cref="SourceClock"/>, if it hasn't been given a source yet.
        /// This is usually done before a seek to avoid accidentally seeking only the adjustable source in decoupled mode,
        /// but not the actual source clock.
        /// That will pretty much only happen on the very first call of this method, as the source clock is passed in the constructor,
        /// but it is not yet set on the adjustable source there.
        /// </summary>
        private void ensureSourceClockSet()
        {
            if (decoupledClock.Source == null)
                ChangeSource(SourceClock);
        }

        protected override void Update()
        {
            if (!IsPaused.Value)
                FramedClock.ProcessFrame();

            base.Update();
        }

        /// <summary>
        /// Invoked when the value of <see cref="IsPaused"/> is changed to start or stop the <see cref="decoupledClock"/> clock.
        /// </summary>
        /// <param name="isPaused">Whether the clock should now be paused.</param>
        protected virtual void OnIsPausedChanged(ValueChangedEvent<bool> isPaused)
        {
            if (isPaused.NewValue)
                decoupledClock.Stop();
            else
                decoupledClock.Start();
        }

        /// <summary>
        /// Creates the final <see cref="FramedClock"/> which is exposed via DI to be used by gameplay components.
        /// </summary>
        /// <remarks>
        /// Any intermediate clocks such as platform offsets should be applied here.
        /// </remarks>
        /// <param name="source">The <see cref="IFrameBasedClock"/> providing the source time.</param>
        /// <returns>The final <see cref="FramedClock"/>.</returns>
        protected virtual IFrameBasedClock CreateGameplayClock(IFrameBasedClock source) => source;

        #region IAdjustableClock

        bool IAdjustableClock.Seek(double position)
        {
            Seek(position);
            return true;
        }

        void IAdjustableClock.Reset() => Reset();

        public void ResetSpeedAdjustments() => throw new NotImplementedException();

        double IAdjustableClock.Rate
        {
            get => FramedClock.Rate;
            set => throw new NotSupportedException();
        }

        public double Rate => FramedClock.Rate;

        public double CurrentTime => FramedClock.CurrentTime;

        public bool IsRunning => FramedClock.IsRunning;

        #endregion

        public void ProcessFrame()
        {
            // Handled via update. Don't process here to safeguard from external usages potentially processing frames additional times.
        }

        public double ElapsedFrameTime => FramedClock.ElapsedFrameTime;

        public double FramesPerSecond => FramedClock.FramesPerSecond;

        public FrameTimeInfo TimeInfo => FramedClock.TimeInfo;

        public double TrueGameplayRate
        {
            get
            {
                double baseRate = Rate;

                foreach (double adjustment in NonGameplayAdjustments)
                {
                    if (Precision.AlmostEquals(adjustment, 0))
                        return 0;

                    baseRate /= adjustment;
                }

                return baseRate;
            }
        }
    }
}
