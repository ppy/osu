// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public partial class DrawableTopScore : CompositeDrawable
    {
        private readonly Box background;

        public DrawableTopScore(ScoreInfo score, int? position = 1)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 4;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(0.2f),
                Radius = 1,
                Offset = new Vector2(0, 1),
            };

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding
                    {
                        Vertical = 10,
                        Left = 10,
                        Right = 30,
                    },
                    Children = new Drawable[]
                    {
                        new AutoSizingGrid
                        {
                            RelativeSizeAxes = Axes.X,
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new TopScoreUserSection
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Score = score,
                                        ScorePosition = position,
                                    },
                                    null,
                                    new TopScoreStatisticsSection
                                    {
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                        Score = score,
                                    }
                                },
                            },
                            ColumnDimensions = new[] { new Dimension(GridSizeMode.AutoSize), new Dimension(GridSizeMode.Absolute, 20) },
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Background4;
        }

        private partial class AutoSizingGrid : GridContainer
        {
            public AutoSizingGrid()
            {
                AutoSizeAxes = Axes.Y;
            }
        }
    }
}
