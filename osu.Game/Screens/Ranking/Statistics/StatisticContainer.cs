// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Screens.Ranking.Statistics
{
    public class StatisticContainer : Container
    {
        protected override Container<Drawable> Content => content;

        private readonly Container content;

        public StatisticContainer(string name)
        {
            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5, 0),
                            Children = new Drawable[]
                            {
                                new Circle
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Height = 9,
                                    Width = 4,
                                    Colour = Color4Extensions.FromHex("#00FFAA")
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Text = name,
                                    Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold),
                                }
                            }
                        }
                    },
                    new Drawable[]
                    {
                        content = new Container
                        {
                            RelativeSizeAxes = Axes.Both
                        }
                    },
                },
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                }
            };
        }
    }
}
