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
using osu.Game.Graphics.Backgrounds;
using osu.Game.Screens.Menu;

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
        private double lastActionTime = 0;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.75f,
                },
                new SpriteText
                {
                    Text = @"paused",
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.Centre,
                    Position = new Vector2(0, -175),
                    Font = @"Exo2.0-Medium",
                    Spacing = new Vector2(5, 0),
                    TextSize = 30,
                    Colour = colours.Yellow,
                    Shadow = true,
                    ShadowColour = new Color4(0, 0, 0, 0.25f),
                },
                new SpriteText
                {
                    Text = @"you're not going to do what i think you're going to do, ain't ya?",
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.Centre,
                    Width = 100,
                    Position = new Vector2(0, -125),
                    Shadow = true,
                    ShadowColour = new Color4(0, 0, 0, 0.25f),
                },
                new SpriteText
                {
                    Text = @"You've retried 0 times in this session",
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.Centre,
                    Width = 100,
                    Position = new Vector2(0, 175),
                    Shadow = true,
                    ShadowColour = new Color4(0, 0, 0, 0.25f),
                    TextSize = 18,
                },
                new FlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Position = new Vector2(0, 25),
                    Masking = true,
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = new Color4(0, 0, 0, 150),
                        Radius = 50,
                        Offset = new Vector2(0, 0),
                    },

                    Children = new Drawable[]
                    {
                        new PauseButton
                        {
                            Type = PauseButtonType.Resume,
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Action = Resume,
                        },
                        new PauseButton
                        {
                            Type = PauseButtonType.Retry,
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Action = Retry,
                        },
                        new PauseButton
                        {
                            Type = PauseButtonType.Quit,
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Action = Quit,
                        },
                        new Button(@"solo", @"freeplay", FontAwesome.fa_user, new Color4(102, 68, 204, 255), () => OnPause?.Invoke(), 300, Key.P),
                    }
                },
            };
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
