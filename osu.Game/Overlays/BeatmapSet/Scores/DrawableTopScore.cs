// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class DrawableTopScore : CompositeDrawable
    {
        private const float fade_duration = 100;

        private Color4 backgroundIdleColour;
        private Color4 backgroundHoveredColour;

        private readonly Box background;

        public DrawableTopScore(ScoreInfo score, int position = 1)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 10;
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
                    Padding = new MarginPadding(10),
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
        private void load(OsuColour colours)
        {
            backgroundIdleColour = colours.Gray3;
            backgroundHoveredColour = colours.Gray4;

            background.Colour = backgroundIdleColour;
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(backgroundHoveredColour, fade_duration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeColour(backgroundIdleColour, fade_duration, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        private class AutoSizingGrid : GridContainer
        {
            public AutoSizingGrid()
            {
                AutoSizeAxes = Axes.Y;
            }
        }
    }
}
