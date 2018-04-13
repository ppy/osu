﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public abstract class PlayerSettingsGroup : Container
    {
        /// <summary>
        /// The title to be displayed in the header of this group.
        /// </summary>
        protected abstract string Title { get; }

        private const float transition_duration = 250;
        private const int container_width = 270;
        private const int border_thickness = 2;
        private const int header_height = 30;
        private const int corner_radius = 5;

        private readonly FillFlowContainer content;
        private readonly IconButton button;

        private bool expanded = true;

        public bool Expanded
        {
            get { return expanded; }
            set
            {
                if (expanded == value) return;
                expanded = value;

                content.ClearTransforms();

                if (expanded)
                    content.AutoSizeAxes = Axes.Y;
                else
                {
                    content.AutoSizeAxes = Axes.None;
                    content.ResizeHeightTo(0, transition_duration, Easing.OutQuint);
                }

                button.FadeColour(expanded ? buttonActiveColour : Color4.White, 200, Easing.OutQuint);
            }
        }

        private Color4 buttonActiveColour;

        protected PlayerSettingsGroup()
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
                                button = new IconButton
                                {
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.CentreRight,
                                    Position = new Vector2(-15, 0),
                                    Icon = FontAwesome.fa_bars,
                                    Scale = new Vector2(0.75f),
                                    Action = () => Expanded = !Expanded,
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
                            AutoSizeEasing = Easing.OutQuint,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding(15),
                            Spacing = new Vector2(0, 15),
                        }
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            button.Colour = buttonActiveColour = colours.Yellow;
        }

        protected override Container<Drawable> Content => content;

        protected override bool OnHover(InputState state) => true;
        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => true;
    }
}
