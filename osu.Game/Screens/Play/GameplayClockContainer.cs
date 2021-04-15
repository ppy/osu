// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;

namespace osu.Game.Screens.Play
{
    public abstract class GameplayClockContainer : Container
    {
        /// <summary>
        /// The final clock which is exposed to underlying components.
        /// </summary>
        public GameplayClock GameplayClock { get; private set; }

        public readonly BindableBool IsPaused = new BindableBool();

        /// <summary>
        /// The decoupled clock used for gameplay. Should be used for seeks and clock control.
        /// </summary>
        protected readonly DecoupleableInterpolatingFramedClock AdjustableClock;

        protected GameplayClockContainer(IClock sourceClock)
        {
            RelativeSizeAxes = Axes.Both;

            AdjustableClock = new DecoupleableInterpolatingFramedClock { IsCoupled = false };
            AdjustableClock.ChangeSource(sourceClock);

            IsPaused.BindValueChanged(OnIsPausedChanged);
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            dependencies.CacheAs(GameplayClock = CreateGameplayClock(AdjustableClock));
            GameplayClock.IsPaused.BindTo(IsPaused);

            return dependencies;
        }

        public virtual void Start()
        {
            if (!AdjustableClock.IsRunning)
            {
                // Seeking the decoupled clock to its current time ensures that its source clock will be seeked to the same time
                // This accounts for the clock source potentially taking time to enter a completely stopped state
                Seek(GameplayClock.CurrentTime);

                AdjustableClock.Start();
            }

            IsPaused.Value = false;
        }

        /// <summary>
        /// Seek to a specific time in gameplay.
        /// <remarks>
        /// Adjusts for any offsets which have been applied (so the seek may not be the expected point in time on the underlying audio track).
        /// </remarks>
        /// </summary>
        /// <param name="time">The destination time to seek to.</param>
        public virtual void Seek(double time) => AdjustableClock.Seek(time);

        public virtual void Stop() => IsPaused.Value = true;

        public virtual void Restart()
        {
            AdjustableClock.Seek(0);
            AdjustableClock.Stop();

            if (!IsPaused.Value)
                Start();
        }

        protected override void Update()
        {
            if (!IsPaused.Value)
                ClockToProcess.ProcessFrame();

            base.Update();
        }

        protected abstract void OnIsPausedChanged(ValueChangedEvent<bool> isPaused);

        protected virtual IFrameBasedClock ClockToProcess => AdjustableClock;

        protected abstract GameplayClock CreateGameplayClock(IFrameBasedClock source);
    }
}
