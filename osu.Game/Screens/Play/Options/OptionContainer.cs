// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Play.Options
{
    public abstract class OptionContainer : Container
    {
        /// <summary>
        /// The title of this option.
        /// </summary>
        public abstract string Title { get; }

        private Container header;
        private FillFlowContainer content;

        public OptionContainer()
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
                        header = new Container
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
                                new SimpleButton
                                {
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.CentreRight,
                                    Position = new Vector2(-15,0),
                                    Icon = FontAwesome.fa_bars,
                                    Scale = new Vector2(0.7f),
                                },
                            }
                        },
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
                    }
                },
            };
        }

        public new void Add(Drawable drawable)
        {
            content.Add(drawable);
        }
    }
}
