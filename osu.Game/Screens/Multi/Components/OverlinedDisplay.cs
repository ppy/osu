// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Screens.Multi.Components
{
    public abstract class OverlinedDisplay : MultiplayerComposite
    {
        protected readonly Container Content;

        public override Axes RelativeSizeAxes
        {
            get => base.RelativeSizeAxes;
            set
            {
                base.RelativeSizeAxes = value;
                updateDimensions();
            }
        }

        public override Axes AutoSizeAxes
        {
            get => base.AutoSizeAxes;
            protected set
            {
                base.AutoSizeAxes = value;
                updateDimensions();
            }
        }

        protected string Details
        {
            set => details.Text = value;
        }

        private readonly Circle line;
        private readonly OsuSpriteText details;
        private readonly GridContainer grid;

        protected OverlinedDisplay(string title)
        {
            InternalChild = grid = new GridContainer
            {
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
                        Content = new Container { Margin = new MarginPadding { Top = 5 } }
                    }
                }
            };

            updateDimensions();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            line.Colour = colours.Yellow;
            details.Colour = colours.Yellow;
        }

        private void updateDimensions()
        {
            grid.RowDimensions = new[]
            {
                new Dimension(GridSizeMode.AutoSize),
                new Dimension(GridSizeMode.AutoSize),
                new Dimension(AutoSizeAxes.HasFlag(Axes.Y) ? GridSizeMode.AutoSize : GridSizeMode.Distributed),
            };

            grid.AutoSizeAxes = Axes.None;
            grid.RelativeSizeAxes = Axes.None;
            grid.AutoSizeAxes = AutoSizeAxes;
            grid.RelativeSizeAxes = ~AutoSizeAxes;

            Content.AutoSizeAxes = Axes.None;
            Content.RelativeSizeAxes = Axes.None;
            Content.AutoSizeAxes = grid.AutoSizeAxes;
            Content.RelativeSizeAxes = grid.RelativeSizeAxes;
        }
    }
}
