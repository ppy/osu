// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// A container which handles pausing children, displaying a pause overlay with choices etc.
    /// This alleviates a lot of the intricate pause logic from being in <see cref="Player"/>
    /// </summary>
    public class PauseContainer : Container
    {
        public readonly BindableBool IsPaused = new BindableBool();

        public Func<bool> CheckCanPause;

        private const double pause_cooldown = 1000;
        private double lastPauseActionTime;

        private readonly PauseOverlay pauseOverlay;

        private readonly Container content;

        protected override Container<Drawable> Content => content;

        public int Retries
        {
            set => pauseOverlay.Retries = value;
        }

        public bool CanPause => (CheckCanPause?.Invoke() ?? true) && Time.Current >= lastPauseActionTime + pause_cooldown;
        public bool IsResuming { get; private set; }

        public Action OnRetry;
        public Action OnQuit;

        private readonly FramedClock offsetClock;
        private readonly DecoupleableInterpolatingFramedClock adjustableClock;

        /// <summary>
        /// The final clock which is exposed to underlying components.
        /// </summary>
        [Cached]
        private readonly GameplayClock gameplayClock;

        /// <summary>
        /// Creates a new <see cref="PauseContainer"/>.
        /// </summary>
        /// <param name="offsetClock">The gameplay clock. This is the clock that will process frames. Includes user/system offsets.</param>
        /// <param name="adjustableClock">The seekable clock. This is the clock that will be paused and resumed. Should not be processed (it is processed automatically by <see cref="offsetClock"/>).</param>
        public PauseContainer(FramedClock offsetClock, DecoupleableInterpolatingFramedClock adjustableClock)
        {
            this.offsetClock = offsetClock;
            this.adjustableClock = adjustableClock;

            gameplayClock = new GameplayClock(offsetClock);

            RelativeSizeAxes = Axes.Both;

            AddInternal(content = new Container
            {
                Clock = this.offsetClock,
                ProcessCustomClock = false,
                RelativeSizeAxes = Axes.Both
            });

            AddInternal(pauseOverlay = new PauseOverlay
            {
                OnResume = () =>
                {
                    IsResuming = true;
                    this.Delay(400).Schedule(Resume);
                },
                OnRetry = () => OnRetry(),
                OnQuit = () => OnQuit(),
            });
        }

        public void Pause(bool force = false) => Schedule(() => // Scheduled to ensure a stable position in execution order, no matter how it was called.
        {
            if (!CanPause && !force) return;

            if (IsPaused.Value) return;

            // stop the seekable clock (stops the audio eventually)
            adjustableClock.Stop();
            IsPaused.Value = true;

            pauseOverlay.Show();

            lastPauseActionTime = Time.Current;
        });

        public void Resume()
        {
            if (!IsPaused.Value) return;

            IsPaused.Value = false;
            IsResuming = false;
            lastPauseActionTime = Time.Current;

            // Seeking the decoupled clock to its current time ensures that its source clock will be seeked to the same time
            // This accounts for the audio clock source potentially taking time to enter a completely stopped state
            adjustableClock.Seek(adjustableClock.CurrentTime);
            adjustableClock.Start();

            pauseOverlay.Hide();
        }

        private OsuGameBase game;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            this.game = game;
        }

        protected override void Update()
        {
            // eagerly pause when we lose window focus (if we are locally playing).
            if (!game.IsActive.Value && CanPause)
                Pause();

            if (!IsPaused.Value)
                offsetClock.ProcessFrame();

            base.Update();
        }

        public class PauseOverlay : GameplayMenuOverlay
        {
            public Action OnResume;

            public override string Header => "paused";
            public override string Description => "you're not going to do what i think you're going to do, are ya?";

            protected override Action BackAction => () => InternalButtons.Children.First().Click();

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AddButton("Continue", colours.Green, () => OnResume?.Invoke());
                AddButton("Retry", colours.YellowDark, () => OnRetry?.Invoke());
                AddButton("Quit", new Color4(170, 27, 39, 255), () => OnQuit?.Invoke());
            }
        }
    }

    /// <summary>
    /// A clock which is used for gameplay elements that need to follow audio time 1:1.
    /// Exposed via DI by <see cref="PauseContainer"/>.
    /// <remarks>
    /// THe main purpose of this clock is to stop components using it from accidentally processing the main
    /// <see cref="IFrameBasedClock"/>, as this should only be done once to ensure accuracy.
    /// </remarks>
    /// </summary>
    public class GameplayClock : IFrameBasedClock
    {
        private readonly IFrameBasedClock underlyingClock;

        public GameplayClock(IFrameBasedClock underlyingClock)
        {
            this.underlyingClock = underlyingClock;
        }

        public double CurrentTime => underlyingClock.CurrentTime;

        public double Rate => underlyingClock.Rate;

        public bool IsRunning => underlyingClock.IsRunning;

        public void ProcessFrame()
        {
            // we do not want to process the underlying clock.
            // this is handled by PauseContainer.
        }

        public double ElapsedFrameTime => underlyingClock.ElapsedFrameTime;

        public double FramesPerSecond => underlyingClock.FramesPerSecond;

        public FrameTimeInfo TimeInfo => underlyingClock.TimeInfo;
    }
}
