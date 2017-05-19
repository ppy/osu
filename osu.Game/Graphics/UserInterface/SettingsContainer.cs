// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public abstract class SettingsContainer : Container
    {
        /// <summary>
        /// The title of this container.
        /// </summary>
        public abstract string Title { get; }

        private const float transition_duration = 600;
        private const int container_width = 250;
        private const int border_thickness = 2;
        private const int header_height = 30;
        private const int corner_radius = 5;

        private readonly FillFlowContainer content;
        private readonly SimpleButton button;
        private bool buttonIsPressed;

        protected SettingsContainer()
        {
            AutoSizeAxes = Axes.Y;
            Width = container_width;
            Masking = true;
            CornerRadius = corner_radius;
            BorderColour = Color4.Black;
            BorderThickness = border_thickness;

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
                            Height = header_height,
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
                        content = new FillFlowContainer
                        {
                            Direction = FillDirection.Vertical,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            AutoSizeDuration = transition_duration,
                            AutoSizeEasing = EasingTypes.OutQuint,
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Padding = new MarginPadding(15),
                            Spacing = new Vector2(0, 10),
                        }
                    }
                },
            };

            button.TriggerClick();
        }

        public new void Add(Drawable drawable)
        {
            content.Add(drawable);
        }

        private void triggerContentVisibility()
        {
            buttonIsPressed = !buttonIsPressed;

            button.FadeColour(buttonIsPressed ? OsuColour.FromHex(@"ffcc22") : Color4.White, 200, EasingTypes.OutQuint);

            if (buttonIsPressed)
            {
                content.ClearTransforms();
                content.AutoSizeAxes = Axes.Y;
            }
            else
            {
                content.AutoSizeAxes = Axes.None;
                content.ResizeHeightTo(0, transition_duration, EasingTypes.OutQuint);
            }
        }
    }
}
