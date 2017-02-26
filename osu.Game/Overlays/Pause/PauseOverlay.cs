// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Pause
{
    public class PauseOverlay : OverlayContainer
    {
        private const int transition_duration = 200;
        private const int button_height = 70;
        private const float background_alpha = 0.75f;

        protected override bool HideOnEscape => false;

        public Action OnResume;
        public Action OnRetry;
        public Action OnQuit;

        public int Retries
        {
            set
            {
                if (retryCounterContainer != null)
                {
                    // "You've retried 1,065 times in this session"
                    // "You've retried 1 time in this session"

                    retryCounterContainer.Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = "You've retried ",
                            Shadow = true,
                            ShadowColour = new Color4(0, 0, 0, 0.25f),
                            TextSize = 18
                        },
                        new OsuSpriteText
                        {
                            Text = String.Format("{0:n0}", value),
                            Font = @"Exo2.0-Bold",
                            Shadow = true,
                            ShadowColour = new Color4(0, 0, 0, 0.25f),
                            TextSize = 18
                        },
                        new OsuSpriteText
                        {
                            Text = $" time{((value == 1) ? "" : "s")} in this session",
                            Shadow = true,
                            ShadowColour = new Color4(0, 0, 0, 0.25f),
                            TextSize = 18
                        }
                    };
                }
            }
        }

        private FlowContainer retryCounterContainer;

        public override bool Contains(Vector2 screenSpacePos) => true;
        public override bool HandleInput => State == Visibility.Visible;

        protected override void PopIn() => FadeIn(transition_duration, EasingTypes.In);
        protected override void PopOut() => FadeOut(transition_duration, EasingTypes.In);

        // Don't let mouse down events through the overlay or people can click circles while paused.
        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => true;

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Key == Key.Escape)
            {
                if (State == Visibility.Hidden) return false;
                resume();
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
                    Alpha = background_alpha,
                },
                new FlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FlowDirections.Vertical,
                    Spacing = new Vector2(0f, 50f),
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new FlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FlowDirections.Vertical,
                            Spacing = new Vector2(0f, 20f),
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
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
                                new OsuSpriteText
                                {
                                    Text = @"you're not going to do what i think you're going to do, are ya?",
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
                                Colour = Color4.Black.Opacity(0.6f),
                                Radius = 50
                            },
                            Children = new Drawable[]
                            {
                                new ResumeButton
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Height = button_height,
                                    Action = resume
                                },
                                new RetryButton
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Height = button_height,
                                    Action = delegate
                                    {
                                        OnRetry?.Invoke();
                                        Hide();
                                    }
                                },
                                new QuitButton
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Height = button_height,
                                    Action = delegate
                                    {
                                        OnQuit?.Invoke();
                                        Hide();
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

            Retries = 0;
        }

        private void resume()
        {
            OnResume?.Invoke();
            Hide();
        }

        public PauseOverlay()
        {
            RelativeSizeAxes = Axes.Both;
        }
    }
}
