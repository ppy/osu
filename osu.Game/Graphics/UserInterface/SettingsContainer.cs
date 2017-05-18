// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public abstract class SettingsContainer : Container
    {
        /// <summary>
        /// The title of this option.
        /// </summary>
        public abstract string Title { get; }

        private readonly SettingsDropdown content;
        private readonly SimpleButton button;
        private bool contentIsVisible;

        protected SettingsContainer()
        {
            AutoSizeAxes = Axes.Y;
            Width = 250;
            Masking = true;
            CornerRadius = 5;
            BorderColour = Color4.Black;
            BorderThickness = 2;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.5f,
                },
                new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,

                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 30,
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,

                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Origin = Anchor.CentreLeft,
                                    Anchor = Anchor.CentreLeft,
                                    Text = Title,
                                    TextSize = 17,
                                    Font = @"Exo2.0-Bold",
                                    Margin = new MarginPadding { Left = 10 },
                                },
                                button = new SimpleButton
                                {
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.CentreRight,
                                    Position = new Vector2(-15,0),
                                    Icon = FontAwesome.fa_bars,
                                    Scale = new Vector2(0.7f),
                                    Action = () => triggerContentVisibility(),
                                },
                            }
                        },
                        content = new SettingsDropdown
                        {
                            RelativeSizeAxes = Axes.X,
                        }
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            content.StateChanged += (c, s) => button.FadeColour(s == Visibility.Visible ? colours.Yellow : Color4.White, 200, EasingTypes.OutQuint);
        }

        public new void Add(Drawable drawable)
        {
            content.Add(drawable);
        }

        private void triggerContentVisibility()
        {
            contentIsVisible = !contentIsVisible;

            if (contentIsVisible)
                content.Show();
            else
                content.Hide();
        }

        private class SettingsDropdown : OverlayContainer
        {
            private const float transition_duration = 600;

            private FillFlowContainer content;

            public SettingsDropdown()
            {
                Children = new Drawable[]
                {
                    content = new FillFlowContainer
                    {
                        Direction = FillDirection.Vertical,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Padding = new MarginPadding(15),
                        Spacing = new Vector2(0, 10),
                    }
                };
            }

            public new void Add(Drawable drawable)
            {
                content.Add(drawable);
            }

            protected override void PopIn()
            {
                ResizeTo(new Vector2(1, content.Height), transition_duration, EasingTypes.OutQuint);
                FadeIn(transition_duration, EasingTypes.OutQuint);
            }

            protected override void PopOut()
            {
                ResizeTo(new Vector2(1, 0), transition_duration, EasingTypes.OutQuint);
                FadeOut(transition_duration);
            }
        }
    }
}
