// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Users;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Extensions;

namespace osu.Game.Screens.Ranking
{
    public class ResultsPageScore : ResultsPage
    {
        private ScoreCounter scoreCounter;

        public ResultsPageScore(Score score, WorkingBeatmap beatmap) : base(score, beatmap) { }

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
                        new DrawableRank(Score.Rank)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Size = new Vector2(150, 60),
                            Margin = new MarginPadding(20),
                        },
                        new Container
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
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
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
                            Font = @"Exo2.0-Bold",
                            TextSize = 16,
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
                        }
                    }
                }
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

        private class DrawableScoreStatistic : Container
        {
            private readonly KeyValuePair<HitResult, object> statistic;

            public DrawableScoreStatistic(KeyValuePair<HitResult, object> statistic)
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
                    new OsuSpriteText {
                        Text = statistic.Value.ToString().PadLeft(4, '0'),
                        Colour = colours.Gray7,
                        TextSize = 30,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                    },
                    new OsuSpriteText {
                        Text = statistic.Key.GetDescription(),
                        Colour = colours.Gray7,
                        Font = @"Exo2.0-Bold",
                        Y = 26,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                    },
                };
            }
        }

        private class DateTimeDisplay : Container
        {
            private DateTime datetime;

            public DateTimeDisplay(DateTime datetime)
            {
                this.datetime = datetime;

                AutoSizeAxes = Axes.Y;

                Width = 140;

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
                    new OsuSpriteText
                    {
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                        Text = datetime.ToShortDateString(),
                        Padding = new MarginPadding { Horizontal = 10, Vertical = 5 },
                        Colour = Color4.White,
                    },
                    new OsuSpriteText
                    {
                        Origin = Anchor.CentreRight,
                        Anchor = Anchor.CentreRight,
                        Text = datetime.ToShortTimeString(),
                        Padding = new MarginPadding { Horizontal = 10, Vertical = 5 },
                        Colour = Color4.White,
                    }
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
                                TextSize = 24,
                                Font = @"Exo2.0-BoldItalic",
                            },
                            artist = new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Shadow = false,
                                TextSize = 20,
                                Font = @"Exo2.0-BoldItalic",
                            },
                            versionMapper = new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Shadow = false,
                                TextSize = 16,
                                Font = @"Exo2.0-Bold",
                            },
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, LocalisationEngine localisation)
            {
                title.Colour = artist.Colour = colours.BlueDarker;
                versionMapper.Colour = colours.Gray8;

                var creator = beatmap.Metadata.Author?.Username;
                if (!string.IsNullOrEmpty(creator)) {
                    versionMapper.Text = $"mapped by {creator}";

                    if (!string.IsNullOrEmpty(beatmap.Version))
                        versionMapper.Text = $"{beatmap.Version} - " + versionMapper.Text;
                }

                title.Current = localisation.GetUnicodePreference(beatmap.Metadata.TitleUnicode, beatmap.Metadata.Title);
                artist.Current = localisation.GetUnicodePreference(beatmap.Metadata.ArtistUnicode, beatmap.Metadata.Artist);
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
                        Font = @"Exo2.0-RegularItalic",
                        Text = user.Username,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        TextSize = 30,
                        Padding = new MarginPadding { Bottom = 10 },
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                if (!string.IsNullOrEmpty(user.CoverUrl))
                    cover.Texture = textures.Get(user.CoverUrl);
            }
        }

        private class SlowScoreCounter : ScoreCounter
        {
            protected override double RollingDuration => 3000;

            protected override Easing RollingEasing => Easing.OutPow10;

            public SlowScoreCounter(uint leading = 0) : base(leading)
            {
                DisplayedCountSpriteText.Shadow = false;
                DisplayedCountSpriteText.Font = @"Venera-Light";
                UseCommaSeparator = true;
            }
        }
    }
}
