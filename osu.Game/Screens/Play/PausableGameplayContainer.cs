// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// A container which handles pausing children, displaying an overlay blocking its children during paused state.
    /// </summary>
    public class PausableGameplayContainer : Container
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

        public Action Stop;
        public Action Start;

        /// <summary>
        /// Creates a new <see cref="PausableGameplayContainer"/>.
        /// </summary>
        public PausableGameplayContainer()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both
                },
                pauseOverlay = new PauseOverlay
                {
                    OnResume = () =>
                    {
                        IsResuming = true;
                        this.Delay(400).Schedule(Resume);
                    },
                    OnRetry = () => OnRetry(),
                    OnQuit = () => OnQuit(),
                }
            };
        }

        public void Pause(bool force = false) => Schedule(() => // Scheduled to ensure a stable position in execution order, no matter how it was called.
        {
            if (!CanPause && !force) return;

            if (IsPaused.Value) return;

            // stop the seekable clock (stops the audio eventually)
            Stop?.Invoke();
            IsPaused.Value = true;

            pauseOverlay.Show();

            lastPauseActionTime = Time.Current;
        });

        public void Resume()
        {
            if (!IsPaused.Value) return;

            IsResuming = false;
            lastPauseActionTime = Time.Current;

            IsPaused.Value = false;

            Start?.Invoke();

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
}
