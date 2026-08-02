// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Screens.Select;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class GameplayWarmupScreen
    {
        private partial class TitleWedge : CompositeDrawable
        {
            private const float corner_radius = 10;

            private readonly APIBeatmap beatmap;

            private BeatmapSetOnlineStatusPill statusPill = null!;
            private MarqueeContainer titleLabel = null!;
            private MarqueeContainer artistLabel = null!;

            private BeatmapTitleWedge.StatisticPlayCount playCount = null!;
            private BeatmapTitleWedge.FavouriteButton favouriteButton = null!;
            private BeatmapTitleWedge.Statistic lengthStatistic = null!;
            private BeatmapTitleWedge.Statistic bpmStatistic = null!;

            public TitleWedge(APIBeatmap beatmap)
            {
                this.beatmap = beatmap;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Masking = true;
                Shear = OsuGame.SHEAR;
                CornerRadius = corner_radius;

                InternalChildren = new Drawable[]
                {
                    new WedgeBackground(),
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding
                        {
                            Top = SongSelect.WEDGE_CONTENT_MARGIN,
                            Left = SongSelect.WEDGE_CONTENT_MARGIN
                        },
                        Spacing = new Vector2(0f, 4f),
                        Children = new Drawable[]
                        {
                            new ShearAligningWrapper(statusPill = new BeatmapSetOnlineStatusPill
                            {
                                Shear = -OsuGame.SHEAR,
                                ShowUnknownStatus = true,
                                TextSize = OsuFont.Style.Caption1.Size,
                                TextPadding = new MarginPadding { Horizontal = 6, Vertical = 1 },
                            }),
                            new ShearAligningWrapper(new Container
                            {
                                Shear = -OsuGame.SHEAR,
                                RelativeSizeAxes = Axes.X,
                                Height = OsuFont.Style.Title.Size,
                                Margin = new MarginPadding { Bottom = -4f },
                                Child = titleLabel = new MarqueeContainer
                                {
                                    OverflowSpacing = 50,
                                }
                            }),
                            new ShearAligningWrapper(new Container
                            {
                                Shear = -OsuGame.SHEAR,
                                RelativeSizeAxes = Axes.X,
                                Height = OsuFont.Style.Heading2.Size,
                                Margin = new MarginPadding { Left = 1f },
                                Child = artistLabel = new MarqueeContainer
                                {
                                    OverflowSpacing = 50,
                                }
                            }),
                            new ShearAligningWrapper(new FillFlowContainer
                            {
                                Shear = -OsuGame.SHEAR,
                                AutoSizeAxes = Axes.X,
                                Height = 30,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(2f, 0f),
                                Children = new Drawable[]
                                {
                                    playCount = new BeatmapTitleWedge.StatisticPlayCount(background: true, leftPadding: SongSelect.WEDGE_CONTENT_MARGIN, minSize: 50f)
                                    {
                                        Margin = new MarginPadding { Left = -SongSelect.WEDGE_CONTENT_MARGIN },
                                    },
                                    favouriteButton = new BeatmapTitleWedge.FavouriteButton(),
                                    lengthStatistic = new BeatmapTitleWedge.Statistic(OsuIcon.Clock),
                                    bpmStatistic = new BeatmapTitleWedge.Statistic(OsuIcon.Metronome)
                                    {
                                        TooltipText = BeatmapsetsStrings.ShowStatsBpm,
                                        Margin = new MarginPadding { Left = 5f },
                                    },
                                },
                            }),
                            new ShearAligningWrapper(new Container
                            {
                                Shear = -OsuGame.SHEAR,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Margin = new MarginPadding { Left = -SongSelect.WEDGE_CONTENT_MARGIN },
                                Padding = new MarginPadding { Right = -SongSelect.WEDGE_CONTENT_MARGIN },
                                Child = new DifficultyDisplay(beatmap),
                            }),
                        },
                    }
                };

                statusPill.Status = beatmap.Status;

                var titleText = new RomanisableString(beatmap.BeatmapSet!.TitleUnicode, beatmap.BeatmapSet.Title);
                titleLabel.CreateContent = () => new OsuSpriteText
                {
                    Text = titleText,
                    Shadow = true,
                    Font = OsuFont.Style.Title,
                };

                var artistText = new RomanisableString(beatmap.BeatmapSet.ArtistUnicode, beatmap.BeatmapSet.Artist);
                artistLabel.CreateContent = () => new OsuSpriteText
                {
                    Text = artistText,
                    Shadow = true,
                    Font = OsuFont.Style.Heading2,
                };

                double rate = ModUtils.CalculateRateWithMods([]); // Todo: mods
                double drainLength = Math.Round(beatmap.Length / rate);
                double hitLength = Math.Round(beatmap.HitLength / rate);

                lengthStatistic.Text = hitLength.ToFormattedDuration();
                lengthStatistic.TooltipText = BeatmapsetsStrings.ShowStatsTotalLength(drainLength.ToFormattedDuration());
                bpmStatistic.Text = beatmap.BPM.ToLocalisableString();

                playCount.Value = new BeatmapTitleWedge.StatisticPlayCount.Data(beatmap.PlayCount, beatmap.UserPlayCount);
                favouriteButton.SetBeatmapSet(beatmap.BeatmapSet);
            }
        }
    }
}
