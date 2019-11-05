// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Ranking.Pages
{
    public class ScoreResultsPage : ResultsPage
    {
        private Container scoreContainer;
        private ScoreCounter scoreCounter;

        private readonly ScoreInfo score;

        public ScoreResultsPage(ScoreInfo score, WorkingBeatmap beatmap)
            : base(score, beatmap)
        {
            this.score = score;
        }

        private FillFlowContainer<DrawableScoreStatistic> statisticsContainer;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            const float user_header_height = 120;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = user_header_height },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                        },
                    }
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new UserHeader(Score.User)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = user_header_height,
                        },
                        new UpdateableRank(Score.Rank)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Size = new Vector2(150, 60),
                            Margin = new MarginPadding(20),
                        },
                        scoreContainer = new Container
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = 60,
                            Children = new Drawable[]
                            {
                                new SongProgressGraph
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0.5f,
                                    Objects = Beatmap.Beatmap.HitObjects,
                                },
                                scoreCounter = new SlowScoreCounter(6)
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Colour = colours.PinkDarker,
                                    Y = 10,
                                    TextSize = 56,
                                },
                            }
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Colour = colours.PinkDarker,
                            Shadow = false,
                            Font = OsuFont.GetFont(weight: FontWeight.Bold),
                            Text = "total score",
                            Margin = new MarginPadding { Bottom = 15 },
                        },
                        new BeatmapDetails(Beatmap.BeatmapInfo)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Margin = new MarginPadding { Bottom = 10 },
                        },
                        new DateTimeDisplay(Score.Date.LocalDateTime)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Size = new Vector2(0.75f, 1),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Margin = new MarginPadding { Top = 10, Bottom = 10 },
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = ColourInfo.GradientHorizontal(
                                        colours.GrayC.Opacity(0),
                                        colours.GrayC.Opacity(0.9f)),
                                    RelativeSizeAxes = Axes.Both,
                                    Size = new Vector2(0.5f, 1),
                                },
                                new Box
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Colour = ColourInfo.GradientHorizontal(
                                        colours.GrayC.Opacity(0.9f),
                                        colours.GrayC.Opacity(0)),
                                    RelativeSizeAxes = Axes.Both,
                                    Size = new Vector2(0.5f, 1),
                                },
                            }
                        },
                        statisticsContainer = new FillFlowContainer<DrawableScoreStatistic>
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Direction = FillDirection.Horizontal,
                            LayoutDuration = 200,
                            LayoutEasing = Easing.OutQuint
                        },
                    },
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Margin = new MarginPadding { Bottom = 10 },
                    Spacing = new Vector2(5),
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        new ReplayDownloadButton(score),
                        new RetryButton()
                    }
                },
            };

            statisticsContainer.ChildrenEnumerable = Score.Statistics.OrderByDescending(p => p.Key).Select(s => new DrawableScoreStatistic(s));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Schedule(() =>
            {
                scoreCounter.Increment(Score.TotalScore);

                int delay = 0;

                foreach (var s in statisticsContainer.Children)
                {
                    s.FadeOut()
                     .Then(delay += 200)
                     .FadeIn(300 + delay, Easing.Out);
                }
            });
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            scoreCounter.Scale = new Vector2(Math.Min(1f, (scoreContainer.DrawWidth - 20) / scoreCounter.DrawWidth));
        }

        private class DrawableScoreStatistic : Container
        {
            private readonly KeyValuePair<HitResult, int> statistic;

            public DrawableScoreStatistic(KeyValuePair<HitResult, int> statistic)
            {
                this.statistic = statistic;

                AutoSizeAxes = Axes.Both;
                Margin = new MarginPadding { Left = 5, Right = 5 };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = statistic.Value.ToString().PadLeft(4, '0'),
                        Colour = colours.Gray7,
                        Font = OsuFont.GetFont(size: 30),
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                    },
                    new OsuSpriteText
                    {
                        Text = statistic.Key.GetDescription(),
                        Colour = colours.Gray7,
                        Font = OsuFont.GetFont(weight: FontWeight.Bold),
                        Y = 26,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                    },
                };
            }
        }

        private class DateTimeDisplay : Container
        {
            private readonly DateTime date;

            public DateTimeDisplay(DateTime date)
            {
                this.date = date;

                AutoSizeAxes = Axes.Both;

                Masking = true;
                CornerRadius = 5;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.Gray6,
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Padding = new MarginPadding { Horizontal = 10, Vertical = 5 },
                        Spacing = new Vector2(10),
                        Children = new[]
                        {
                            new OsuSpriteText
                            {
                                Text = date.ToShortDateString(),
                                Colour = Color4.White,
                            },
                            new OsuSpriteText
                            {
                                Text = date.ToShortTimeString(),
                                Colour = Color4.White,
                            }
                        }
                    },
                };
            }
        }

        private class BeatmapDetails : Container
        {
            private readonly BeatmapInfo beatmap;

            private readonly OsuSpriteText title;
            private readonly OsuSpriteText artist;
            private readonly OsuSpriteText versionMapper;

            public BeatmapDetails(BeatmapInfo beatmap)
            {
                this.beatmap = beatmap;

                AutoSizeAxes = Axes.Both;

                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Direction = FillDirection.Vertical,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            title = new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Shadow = false,
                                Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 24, italics: true),
                            },
                            artist = new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Shadow = false,
                                Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 20, italics: true),
                            },
                            versionMapper = new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Shadow = false,
                                Font = OsuFont.GetFont(weight: FontWeight.Bold),
                            },
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                title.Colour = artist.Colour = colours.BlueDarker;
                versionMapper.Colour = colours.Gray8;

                var creator = beatmap.Metadata.Author?.Username;

                if (!string.IsNullOrEmpty(creator))
                {
                    versionMapper.Text = $"mapped by {creator}";

                    if (!string.IsNullOrEmpty(beatmap.Version))
                        versionMapper.Text = $"{beatmap.Version} - " + versionMapper.Text;
                }

                title.Text = new LocalisedString((beatmap.Metadata.TitleUnicode, beatmap.Metadata.Title));
                artist.Text = new LocalisedString((beatmap.Metadata.ArtistUnicode, beatmap.Metadata.Artist));
            }
        }

        private class UserHeader : Container
        {
            private readonly User user;
            private readonly Sprite cover;

            public UserHeader(User user)
            {
                this.user = user;
                Children = new Drawable[]
                {
                    cover = new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fill,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Text = user.Username,
                        Font = OsuFont.GetFont(size: 30, weight: FontWeight.Regular, italics: true),
                        Padding = new MarginPadding { Bottom = 10 },
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(LargeTextureStore textures)
            {
                if (!string.IsNullOrEmpty(user.CoverUrl))
                    cover.Texture = textures.Get(user.CoverUrl);
            }
        }

        private class SlowScoreCounter : ScoreCounter
        {
            protected override double RollingDuration => 3000;

            protected override Easing RollingEasing => Easing.OutPow10;

            public SlowScoreCounter(uint leading = 0)
                : base(leading)
            {
                DisplayedCountSpriteText.Shadow = false;
                DisplayedCountSpriteText.Font = DisplayedCountSpriteText.Font.With(Typeface.Venera, weight: FontWeight.Light);
                UseCommaSeparator = true;
            }
        }
    }
}
