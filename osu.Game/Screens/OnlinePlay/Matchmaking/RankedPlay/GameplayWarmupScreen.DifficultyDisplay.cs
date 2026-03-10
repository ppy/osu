// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Screens.Select;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class GameplayWarmupScreen
    {
        private partial class DifficultyDisplay : CompositeDrawable
        {
            private const float border_weight = 2;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            [Resolved]
            private MultiplayerClient client { get; set; } = null!;

            [Resolved]
            private BeatmapManager beatmapManager { get; set; } = null!;

            [Resolved]
            private RulesetStore rulesets { get; set; } = null!;

            private readonly APIBeatmap beatmap;

            private StarRatingDisplay starRatingDisplay = null!;
            private FillFlowContainer nameLine = null!;
            private OsuSpriteText difficultyText = null!;
            private OsuSpriteText mappedByText = null!;
            private OsuSpriteText mapperText = null!;

            private BeatmapTitleWedge.DifficultyStatisticsDisplay countStatisticsDisplay = null!;
            private BeatmapTitleWedge.DifficultyStatisticsDisplay difficultyStatisticsDisplay = null!;

            public DifficultyDisplay(APIBeatmap beatmap)
            {
                this.beatmap = beatmap;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Masking = true;
                CornerRadius = 10;
                Shear = OsuGame.SHEAR;

                InternalChildren = new Drawable[]
                {
                    new WedgeBackground(),
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new ShearAligningWrapper(new GridContainer
                            {
                                Shear = -OsuGame.SHEAR,
                                AlwaysPresent = true,
                                RelativeSizeAxes = Axes.X,
                                Height = 20,
                                Margin = new MarginPadding { Vertical = 5f },
                                Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN },
                                RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                ColumnDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.AutoSize),
                                    new Dimension(GridSizeMode.Absolute, 6),
                                    new Dimension(),
                                },
                                Content = new[]
                                {
                                    new[]
                                    {
                                        starRatingDisplay = new StarRatingDisplay(default, animated: true)
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                        },
                                        Empty(),
                                        nameLine = new FillFlowContainer
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Horizontal,
                                            Margin = new MarginPadding { Bottom = 2f },
                                            Children = new Drawable[]
                                            {
                                                difficultyText = new TruncatingSpriteText
                                                {
                                                    Anchor = Anchor.BottomLeft,
                                                    Origin = Anchor.BottomLeft,
                                                    Font = OsuFont.Style.Body.With(weight: FontWeight.SemiBold),
                                                },
                                                mappedByText = new OsuSpriteText
                                                {
                                                    Anchor = Anchor.BottomLeft,
                                                    Origin = Anchor.BottomLeft,
                                                    Text = " mapped by ",
                                                    Font = OsuFont.Style.Body,
                                                },
                                                mapperText = new TruncatingSpriteText
                                                {
                                                    Shadow = true,
                                                    Font = OsuFont.Style.Body.With(weight: FontWeight.SemiBold),
                                                },
                                            },
                                        },
                                    }
                                },
                            }),
                            new ShearAligningWrapper(new Container
                            {
                                Shear = -OsuGame.SHEAR,
                                RelativeSizeAxes = Axes.X,
                                Height = 53,
                                Padding = new MarginPadding { Bottom = border_weight, Right = border_weight },
                                Child = new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Masking = true,
                                    CornerRadius = 10 - border_weight,
                                    Shear = OsuGame.SHEAR,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = colourProvider.Background5.Opacity(0.8f),
                                        },
                                        new GridContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Right = 20f, Vertical = 7.5f },
                                            Shear = -OsuGame.SHEAR,
                                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                            ColumnDimensions = new[]
                                            {
                                                new Dimension(),
                                                new Dimension(GridSizeMode.Absolute, 30),
                                                new Dimension(GridSizeMode.AutoSize),
                                            },
                                            Content = new[]
                                            {
                                                new[]
                                                {
                                                    countStatisticsDisplay = new BeatmapTitleWedge.DifficultyStatisticsDisplay
                                                    {
                                                        RelativeSizeAxes = Axes.X,
                                                    },
                                                    Empty(),
                                                    difficultyStatisticsDisplay = new BeatmapTitleWedge.DifficultyStatisticsDisplay(autoSize: true),
                                                }
                                            },
                                        }
                                    },
                                }
                            }),
                        }
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                MultiplayerPlaylistItem item = client.Room!.CurrentPlaylistItem;

                RulesetInfo ruleset = rulesets.GetRuleset(item.RulesetID)!;
                Ruleset rulesetInstance = ruleset.CreateInstance();
                BeatmapInfo? localBeatmap =
                    beatmapManager.QueryBeatmap($@"{nameof(BeatmapInfo.OnlineID)} == $0 AND {nameof(BeatmapInfo.MD5Hash)} == {nameof(BeatmapInfo.OnlineMD5Hash)}", item.BeatmapID);
                WorkingBeatmap workingBeatmap = beatmapManager.GetWorkingBeatmap(localBeatmap);
                IBeatmap playableBeatmap = workingBeatmap.GetPlayableBeatmap(ruleset);

                difficultyText.Text = beatmap.DifficultyName;
                mapperText.Text = beatmap.Metadata.Author.Username;
                starRatingDisplay.Current.Value = new StarDifficulty(beatmap.StarRating, beatmap.MaxCombo ?? 0);

                countStatisticsDisplay.Statistics = playableBeatmap.GetStatistics()
                                                                   .Select(s => new BeatmapTitleWedge.StatisticDifficulty.Data(s.Name, s.BarDisplayLength ?? 0, s.BarDisplayLength ?? 0, 1, s.Content))
                                                                   .ToList();

                difficultyStatisticsDisplay.Statistics = rulesetInstance.GetBeatmapAttributesForDisplay(beatmap, [])
                                                                        .Select(a => new BeatmapTitleWedge.StatisticDifficulty.Data(a))
                                                                        .ToList();
            }

            protected override void Update()
            {
                base.Update();

                difficultyText.MaxWidth = Math.Max(nameLine.DrawWidth - mappedByText.DrawWidth - mapperText.DrawWidth - 20, 0);

                // Use difficulty colour until it gets too dark to be visible against dark backgrounds.
                Color4 col = starRatingDisplay.DisplayedStars.Value >= OsuColour.STAR_DIFFICULTY_DEFINED_COLOUR_CUTOFF ? colours.Orange1 : starRatingDisplay.DisplayedDifficultyColour;

                difficultyText.Colour = col;
                mappedByText.Colour = col;
                countStatisticsDisplay.AccentColour = col;
                difficultyStatisticsDisplay.AccentColour = col;
            }
        }
    }
}
