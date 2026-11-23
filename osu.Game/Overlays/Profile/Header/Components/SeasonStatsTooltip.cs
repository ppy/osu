// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class SeasonStatsTooltip : VisibilityContainer, ITooltip<SeasonTooltipData>
    {
        private OsuSpriteText seasonName = null!;
        private OsuSpriteText rank = null!;
        private Box tierLine = null!;

        private Container badgeContainer = null!;
        private OsuSpriteText divisionName = null!;
        private OsuSpriteText divisionThreshold = null!;

        private DailyChallengeStatsTooltip.StatisticsPiece totalScore = null!;

        private Box background = null!;
        private Box topBackground = null!;

        [BackgroundDependencyLoader]
        private void load()
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

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                topBackground = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Padding = new MarginPadding(15),
                                    Children = new Drawable[]
                                    {
                                        new GridContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                            ColumnDimensions = new[]
                                            {
                                                new Dimension(GridSizeMode.AutoSize),
                                                new Dimension(GridSizeMode.Absolute, size: 10),
                                                new Dimension(GridSizeMode.AutoSize),
                                                new Dimension(GridSizeMode.Absolute, size: 10),
                                                new Dimension(),
                                            },
                                            Content = new[]
                                            {
                                                new Drawable?[]
                                                {
                                                    rank = new OsuSpriteText
                                                    {
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre,
                                                        Font = OsuFont.Torus.With(size: 24),
                                                    },
                                                    null,
                                                    new Container
                                                    {
                                                        RelativeSizeAxes = Axes.Y,
                                                        Width = 3,
                                                        Masking = true,
                                                        CornerRadius = 3,
                                                        Child = tierLine = new Box
                                                        {
                                                            RelativeSizeAxes = Axes.Y,
                                                            Width = 3,
                                                        },
                                                    },
                                                    null,
                                                    new FillFlowContainer
                                                    {
                                                        RelativeSizeAxes = Axes.X,
                                                        AutoSizeAxes = Axes.Y,
                                                        Direction = FillDirection.Vertical,
                                                        Children = new Drawable[]
                                                        {
                                                            badgeContainer = new Container
                                                            {
                                                                Size = SeasonProfileCard.SeasonDivisionBadge.BADGE_SIZE,
                                                                Anchor = Anchor.TopCentre,
                                                                Origin = Anchor.TopCentre,
                                                            },
                                                            divisionName = new OsuSpriteText
                                                            {
                                                                Anchor = Anchor.TopCentre,
                                                                Origin = Anchor.TopCentre,
                                                                Font = OsuFont.Style.Caption1,
                                                                Margin = new MarginPadding { Top = 5 },
                                                            },
                                                            divisionThreshold = new OsuSpriteText
                                                            {
                                                                Anchor = Anchor.TopCentre,
                                                                Origin = Anchor.TopCentre,
                                                                Font = OsuFont.Torus.With(size: 10),
                                                                Margin = new MarginPadding { Top = 3 },
                                                            },
                                                        },
                                                    },
                                                },
                                            },
                                        },
                                        seasonName = new OsuSpriteText
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Font = OsuFont.Style.Caption2,
                                            Margin = new MarginPadding { Top = 10 },
                                        },
                                        new Container { Width = 250 }, // min-width
                                    },
                                },
                            },
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding(15f),
                            Child = totalScore = new DailyChallengeStatsTooltip.StatisticsPiece(UsersStrings.ShowSeasonStatsTotalScore),
                        },
                    },
                },
            };
        }

        public void SetContent(SeasonTooltipData content)
        {
            var statistics = content.Statistics;
            var colourProvider = content.ColourProvider;

            background.Colour = colourProvider.Background4;
            topBackground.Colour = colourProvider.Background5;

            rank.Text = statistics.Rank.ToLocalisableString("\\##,##0");
            rank.Colour = colourProvider.Content2;

            tierLine.Colour = OsuColour.ForRankingTier(statistics.Division.Tier);

            divisionName.Text = content.DivisionName;
            divisionName.Colour = colourProvider.Content2;

            divisionThreshold.Text = UsersStrings.ShowSeasonStatsDivisionTopPercentage(statistics.Division.Threshold.ToLocalisableString("P0"));

            seasonName.Text = statistics.Season.Name;
            seasonName.Colour = colourProvider.Content2;

            totalScore.Value = statistics.TotalScore.ToStandardFormattedString(0);
            totalScore.ValueColour = colourProvider.Content2;

            LoadComponentAsync(new SeasonProfileCard.SeasonDivisionBadge(statistics.Division), b => badgeContainer.Child = b);
        }

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);

        public void Move(Vector2 pos) => Position = pos;
    }

    public record SeasonTooltipData(OverlayColourProvider ColourProvider, APISeasonStatistics Statistics, LocalisableString DivisionName);
}
