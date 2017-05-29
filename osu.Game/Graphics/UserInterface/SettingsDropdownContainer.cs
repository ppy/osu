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
    public abstract class SettingsDropdownContainer : Container
    {
        /// <summary>
        /// The title of this container, which will be written in header.
        /// </summary>
        protected abstract string Title { get; }

        private const float transition_duration = 600;
        private const int container_width = 270;
        private const int border_thickness = 2;
        private const int header_height = 30;
        private const int corner_radius = 5;

        private FillFlowContainer content;
        private SimpleButton button;
        private bool buttonIsActive;

        private Color4 buttonActiveColour;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AutoSizeAxes = Axes.Y;
            Width = container_width;
            Masking = true;
            CornerRadius = corner_radius;
            BorderColour = Color4.Black;
            BorderThickness = border_thickness;

            InternalChildren = new Drawable[]
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
                            Name = @"Header",
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = header_height,

                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Origin = Anchor.CentreLeft,
                                    Anchor = Anchor.CentreLeft,
                                    Text = Title.ToUpper(),
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
                                    Scale = new Vector2(0.75f),
                                    Colour = buttonActiveColour = colours.Yellow,
                                    Action = triggerContentVisibility,
                                },
                            }
                        },
                        content = new FillFlowContainer
                        {
                            Name = @"Content",
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Direction = FillDirection.Vertical,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeDuration = transition_duration,
                            AutoSizeEasing = EasingTypes.OutQuint,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding(15),
                            Spacing = new Vector2(0, 15),
                        }
                    }
                },
            };
        }

        protected override Container<Drawable> Content => content;

        private void triggerContentVisibility()
        {
            content.ClearTransforms();
            content.AutoSizeAxes = buttonIsActive ? Axes.Y : Axes.None;

            if (!buttonIsActive)
                content.ResizeHeightTo(0, transition_duration, EasingTypes.OutQuint);

            button.FadeColour(buttonIsActive ? buttonActiveColour : Color4.White, 200, EasingTypes.OutQuint);

            buttonIsActive = !buttonIsActive;
        }
    }
}
