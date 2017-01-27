using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using osu.Game.Graphics;
using osu.Framework.Audio;
using osu.Framework.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transformations;


namespace osu.Game.Overlays.Pause
{
    public class PauseOverlay : OverlayContainer
    {
        public event Action OnPause;
        public event Action OnResume;
        public event Action OnRetry;
        public event Action OnQuit;

        public bool isPaused = false;

        private int fadeDuration = 100;
        private double pauseCooldown = 1000;
        private double lastActionTime = -1000;

        private PauseButton resumeButton;
        private PauseButton retryButton;
        private PauseButton quitButton;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours)
        {
            var sampleHover = audio.Sample.Get(@"Menu/menuclick");
            var sampleBack = audio.Sample.Get(@"Menu/menuback");
            var samplePlayClick = audio.Sample.Get(@"Menu/menu-play-click");

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.6f,
                },
                resumeButton = new PauseButton
                {
                    Text = @"Resume",
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Position = new Vector2(0, -200),
                    Action = Resume,
                },
                retryButton = new PauseButton
                {
                    Text = @"Retry",
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Action = Retry,
                },
                quitButton = new PauseButton
                {
                    Text = @"Quit",
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Position = new Vector2(0, 200),
                    Action = Quit,
                },
            };

            resumeButton.sampleHover = sampleHover;
            resumeButton.sampleClick = sampleBack;

            retryButton.sampleHover = sampleHover;
            retryButton.sampleClick = samplePlayClick;

            quitButton.sampleHover = sampleHover;
            quitButton.sampleClick = sampleBack;
        }

        protected override void PopIn()
        {
            FadeTo(1, fadeDuration, EasingTypes.In);
            isPaused = true;
        }

        protected override void PopOut()
        {
            FadeTo(0, fadeDuration, EasingTypes.In);
            isPaused = false;
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Escape:
                    TogglePaused();
                    return true;
            }
            return base.OnKeyDown(state, args);
        }

        public void Pause()
        {
            if (Time.Current >= (lastActionTime + pauseCooldown))
            {
                lastActionTime = Time.Current;
                Show();
                OnPause?.Invoke();
            }
            else
            {
                isPaused = false;
            }
        }

        public void Resume()
        {
            lastActionTime = Time.Current;
            Hide();
            OnResume?.Invoke();
        }

        public void TogglePaused()
        {
            isPaused = !isPaused;
            (isPaused ? (Action)Pause : Resume)?.Invoke();
        }

        private void Retry()
        {
            Hide();
            OnRetry?.Invoke();
        }

        private void Quit()
        {
            Hide();
            OnQuit?.Invoke();
        }

        public PauseOverlay()
        {
            RelativeSizeAxes = Axes.Both;
            AutoSizeAxes = Axes.Both;
            Depth = -1;
        }
    }
}
