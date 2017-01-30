using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using osu.Game.Graphics;
using osu.Framework.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transformations;
using System.Threading.Tasks;

namespace osu.Game.Overlays.Pause
{
    public class PauseOverlay : OverlayContainer
    {
        private const int transitionDuration = 200;
        private const int buttonHeight = 70;
        private const float backgroundAlpha = 0.75f;

        public Action OnResume;
        public Action OnRetry;
        public Action OnQuit;

        private FlowContainer retryCounterContainer;

        public override bool Contains(Vector2 screenSpacePos) => true;
        public override bool HandleInput => State == Visibility.Visible;

        protected override void PopIn() => FadeIn(transitionDuration, EasingTypes.In);
        protected override void PopOut() => FadeOut(transitionDuration, EasingTypes.In);

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Escape:
                    if (State == Visibility.Hidden) return false;
                    Hide();
                    OnResume?.Invoke();
                    return true;
            }
            return base.OnKeyDown(state, args);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = backgroundAlpha,
                },
                new FlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FlowDirection.VerticalOnly,
                    Spacing = new Vector2(0f, 50f),
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new FlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FlowDirection.VerticalOnly,
                            Spacing = new Vector2(0f, 20f),
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Children = new Drawable[]
                            {
                                new SpriteText
                                {
                                    Text = @"paused",
                                    Font = @"Exo2.0-Medium",
                                    Spacing = new Vector2(5, 0),
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    TextSize = 30,
                                    Colour = colours.Yellow,
                                    Shadow = true,
                                    ShadowColour = new Color4(0, 0, 0, 0.25f)
                                },
                                new SpriteText
                                {
                                    Text = @"you're not going to do what i think you're going to do, ain't ya?",
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Shadow = true,
                                    ShadowColour = new Color4(0, 0, 0, 0.25f)
                                }
                            }
                        },
                        new FlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
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
                                new ResumeButton
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Height = buttonHeight,
                                    Action = delegate
                                    {
                                        Hide();
                                        OnResume?.Invoke();
                                    }
                                },
                                new RetryButton
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Height = buttonHeight,
                                    Action = delegate
                                    {
                                        Hide();
                                        OnRetry?.Invoke();
                                    }
                                },
                                new QuitButton
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Height = buttonHeight,
                                    Action = delegate
                                    {
                                        Hide();
                                        OnQuit?.Invoke();
                                    }
                                }
                            }
                        },
                        retryCounterContainer = new FlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre
                        }
                    }
                },
                new PauseProgressBar
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Width = 1f
                }
            };

            SetRetries(0);
        }

        public void SetRetries(int count)
        {
            if (retryCounterContainer != null)
            {
                // "You've retried 1,065 times in this session"
                // "You've retried 1 time in this session"

                string leading = "You've retried ";
                string countString = String.Format("{0:n0}", count);
                string trailing = $" time{((count == 1) ? "" : "s")} in this session";

                retryCounterContainer.Children = new Drawable[]
                {
                    new SpriteText
                    {
                        Text = leading,
                        Shadow = true,
                        ShadowColour = new Color4(0, 0, 0, 0.25f),
                        TextSize = 18
                    },
                    new SpriteText
                    {
                        Text = countString,
                        Font = @"Exo2.0-Bold",
                        Shadow = true,
                        ShadowColour = new Color4(0, 0, 0, 0.25f),
                        TextSize = 18
                    },
                    new SpriteText
                    {
                        Text = trailing,
                        Shadow = true,
                        ShadowColour = new Color4(0, 0, 0, 0.25f),
                        TextSize = 18
                    }
                };
            }
        }

        public PauseOverlay()
        {
            RelativeSizeAxes = Axes.Both;
        }
    }
}
