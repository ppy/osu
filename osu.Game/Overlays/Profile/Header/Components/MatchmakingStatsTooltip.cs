// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class MatchmakingStatsTooltip : VisibilityContainer, ITooltip<MatchmakingStatsTooltipData>
    {
        private Box background = null!;
        private Container<TableContainer> tableContainer = null!;

        public MatchmakingStatsTooltip()
        {
            AutoSizeAxes = Axes.Both;
            CornerRadius = 20f;
            Masking = true;

            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(0.25f),
                Radius = 30f,
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                tableContainer = new Container<TableContainer>
                {
                    AutoSizeAxes = Axes.Both,
                    Padding = new MarginPadding(15f),
                }
            };
        }

        public void SetContent(MatchmakingStatsTooltipData content)
        {
            var statistics = content.Statistics;
            var colourProvider = content.ColourProvider;

            background.Colour = colourProvider.Background4;

            tableContainer.Child = new MatchmakingStatsTooltipTable(colourProvider)
            {
                AutoSizeAxes = Axes.Both,
                Columns =
                [
                    new TableColumn(dimension: new Dimension(GridSizeMode.AutoSize)),
                    new TableColumn(dimension: new Dimension(GridSizeMode.AutoSize)),
                    new TableColumn(RankingsStrings.MatchmakingWins, dimension: new Dimension(GridSizeMode.AutoSize)),
                    new TableColumn(RankingsStrings.MatchmakingPlays, dimension: new Dimension(GridSizeMode.AutoSize)),
                    new TableColumn(RankingsStrings.MatchmakingPoints, dimension: new Dimension(GridSizeMode.AutoSize)),
                    new TableColumn(RankingsStrings.MatchmakingRating, dimension: new Dimension(GridSizeMode.AutoSize)),
                ],
                RowSize = new Dimension(GridSizeMode.AutoSize),
                Content = statistics.Select(s => createRow(colourProvider, s)).ToArray().ToRectangular()
            };
        }

        private Drawable[] createRow(OverlayColourProvider colourProvider, APIUserMatchmakingStatistics stat)
        {
            return
            [
                new StatisticText(colourProvider)
                {
                    Text = stat.Pool.Name,
                    Colour = Color4.White
                },
                new StatisticText(colourProvider) { Text = $"#{stat.Rank:N0}" },
                new StatisticText(colourProvider) { Text = stat.FirstPlacements.ToString("N0") },
                new StatisticText(colourProvider) { Text = stat.Plays.ToString("N0") },
                new StatisticText(colourProvider) { Text = stat.TotalPoints.ToString("N0") },
                new StatisticText(colourProvider) { Text = stat.Rating.ToString("N0") + (stat.IsRatingProvisional ? "*" : string.Empty) }
            ];
        }

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);

        public void Move(Vector2 pos) => Position = pos;

        private partial class MatchmakingStatsTooltipTable : TableContainer
        {
            private readonly OverlayColourProvider colourProvider;

            public MatchmakingStatsTooltipTable(OverlayColourProvider colourProvider)
            {
                this.colourProvider = colourProvider;
            }

            protected override Drawable CreateHeader(int index, TableColumn? column)
            {
                return new StatisticText(colourProvider)
                {
                    Text = column?.Header ?? string.Empty,
                };
            }
        }

        private partial class StatisticText : OsuSpriteText
        {
            public StatisticText(OverlayColourProvider colourProvider)
            {
                Font = OsuFont.GetFont(size: 12);
                Padding = new MarginPadding { Horizontal = 5, Vertical = 2 };
                Colour = colourProvider.Content2;
            }
        }
    }

    public record MatchmakingStatsTooltipData(OverlayColourProvider ColourProvider, APIUserMatchmakingStatistics[] Statistics);
}
