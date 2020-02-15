// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Screens.Multi.Match.Components
{
    public abstract class OverlinedDisplay : MultiplayerComposite
    {
        protected readonly Container Content;

        protected string Details
        {
            set => details.Text = value;
        }

        private readonly Circle line;
        private readonly OsuSpriteText details;

        protected OverlinedDisplay(string title)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        line = new Circle
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 2,
                            Margin = new MarginPadding { Bottom = 2 }
                        },
                    },
                    new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Margin = new MarginPadding { Top = 5 },
                            Spacing = new Vector2(10, 0),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = title,
                                    Font = OsuFont.GetFont(size: 14)
                                },
                                details = new OsuSpriteText { Font = OsuFont.GetFont(size: 14) },
                            }
                        },
                    },
                    new Drawable[]
                    {
                        Content = new Container
                        {
                            Margin = new MarginPadding { Top = 5 },
                            RelativeSizeAxes = Axes.Both
                        }
                    }
                },
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.AutoSize),
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            line.Colour = colours.Yellow;
            details.Colour = colours.Yellow;
        }
    }
}
