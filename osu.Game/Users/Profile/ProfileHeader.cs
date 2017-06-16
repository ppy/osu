// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;

namespace osu.Game.Users.Profile
{
    public class ProfileHeader : Container
    {
        private readonly OsuTextFlowContainer infoTextLeft, infoTextRight;
        private readonly FillFlowContainer<SpriteText> scoreText, scoreNumberText;

        private readonly Container coverContainer, chartContainer;
        private readonly Sprite levelBadge;
        private readonly SpriteText levelText;
        private readonly GradeBadge gradeSSPlus, gradeSS, gradeSPlus, gradeS, gradeA;

        private const float cover_height = 350, info_height = 150, info_width = 250, avatar_size = 110, level_position = 30, level_height = 60;
        public ProfileHeader(User user)
        {
            RelativeSizeAxes = Axes.X;
            Height = cover_height + info_height;

            Children = new Drawable[]
            {
                coverContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = cover_height,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            ColourInfo = ColourInfo.GradientVertical(Color4.Black.Opacity(0.1f), Color4.Black.Opacity(0.75f))
                        },
                        new Container
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            X = UserProfileOverlay.CONTENT_X_MARGIN,
                            Y = -20,
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new UpdateableAvatar
                                {
                                    User = user,
                                    Size = new Vector2(avatar_size),
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Masking = true,
                                    CornerRadius = 5,
                                    EdgeEffect = new EdgeEffectParameters
                                    {
                                        Type = EdgeEffectType.Shadow,
                                        Colour = Color4.Black.Opacity(0.25f),
                                        Radius = 4,
                                    },
                                },
                                new Container
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    X = avatar_size + 10,
                                    AutoSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Text = user.Username,
                                            TextSize = 30,
                                            Font = @"Exo2.0-RegularItalic",
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Y = -55
                                        },
                                        new DrawableFlag(user.Country?.FlagName ?? "__")
                                        {
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Width = 30,
                                            Height = 20
                                        }
                                    }
                                }
                            }
                        },
                    }
                },
                infoTextLeft = new OsuTextFlowContainer(t =>
                {
                    t.TextSize = 14;
                    t.Alpha = 0.8f;
                })
                {
                    X = UserProfileOverlay.CONTENT_X_MARGIN,
                    Y = cover_height + 20,
                    Width = info_width,
                    AutoSizeAxes = Axes.Y,
                    ParagraphSpacing = 0.8f,
                    LineSpacing = 0.2f
                },
                infoTextRight = new OsuTextFlowContainer(t =>
                {
                    t.TextSize = 14;
                    t.Font = @"Exo2.0-RegularItalic";
                })
                {
                    X = UserProfileOverlay.CONTENT_X_MARGIN + info_width + 20,
                    Y = cover_height + 20,
                    Width = info_width,
                    AutoSizeAxes = Axes.Y,
                    ParagraphSpacing = 0.8f,
                    LineSpacing = 0.2f
                },
                new Container
                {
                    X = -UserProfileOverlay.CONTENT_X_MARGIN,
                    RelativeSizeAxes = Axes.Y,
                    Width = 280,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Y = level_position,
                            Height = level_height,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = Color4.Black.Opacity(0.5f),
                                    RelativeSizeAxes = Axes.Both
                                },
                                levelBadge = new Sprite
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Height = 50,
                                    Width = 50,
                                    Alpha = 0
                                },
                                levelText = new OsuSpriteText
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Y = 11,
                                    TextSize = 20
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Y = cover_height,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.BottomCentre,
                            Height = cover_height - level_height - level_position - 5,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = Color4.Black.Opacity(0.5f),
                                    RelativeSizeAxes = Axes.Both
                                },
                                scoreText = new FillFlowContainer<SpriteText>
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Padding = new MarginPadding { Horizontal = 20, Vertical = 18 },
                                    Spacing = new Vector2(0, 2)
                                },
                                scoreNumberText = new FillFlowContainer<SpriteText>
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Padding = new MarginPadding { Horizontal = 20, Vertical = 18 },
                                    Spacing = new Vector2(0, 2)
                                },
                                new FillFlowContainer<GradeBadge>
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    Y = -64,
                                    Spacing = new Vector2(20, 0),
                                    Children = new[]
                                    {
                                        gradeSSPlus = new GradeBadge("SSPlus") { Alpha = 0 },
                                        gradeSS = new GradeBadge("SS") { Alpha = 0 },
                                    }
                                },
                                new FillFlowContainer<GradeBadge>
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    Y = -18,
                                    Spacing = new Vector2(20, 0),
                                    Children = new[]
                                    {
                                        gradeSPlus = new GradeBadge("SPlus") { Alpha = 0 },
                                        gradeS = new GradeBadge("S") { Alpha = 0 },
                                        gradeA = new GradeBadge("A") { Alpha = 0 },
                                    }
                                }
                            }
                        },
                        chartContainer = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Height = info_height - 15,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = Color4.Black.Opacity(0.25f),
                                    RelativeSizeAxes = Axes.Both
                                }
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            levelBadge.Texture = textures.Get(@"Profile/levelbadge");
        }

        public void FillFullData(User user)
        {
            coverContainer.Add(new AsyncLoadWrapper(new UserCoverBackground(user)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fill,
                OnLoadComplete = d => d.FadeInFromZero(200)
            })
            {
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                Depth = float.MaxValue
            });

            Action<SpriteText> boldItalic = t =>
            {
                t.Font = @"Exo2.0-BoldItalic";
                t.Alpha = 1;
            };

            if (user.Age != null)
            {
                infoTextLeft.AddText($"{user.Age} years old", boldItalic);
            }
            if (user.Country != null)
            {
                infoTextLeft.AddText(" from ");
                infoTextLeft.AddText(user.Country.FullName, boldItalic);
            }
            infoTextLeft.NewParagraph();

            infoTextLeft.AddText("Joined ");
            infoTextLeft.AddText(user.JoinDate, boldItalic);
            infoTextLeft.NewLine();
            infoTextLeft.AddText("Last seen ");
            infoTextLeft.AddText(user.LastVisit.LocalDateTime.ToShortDateString(), boldItalic);
            infoTextLeft.NewParagraph();

            if (user.PlayStyle?.Length > 0)
            {
                infoTextLeft.AddText("Plays with ");
                infoTextLeft.AddText(string.Join(", ", user.PlayStyle), boldItalic);
            }

            tryAddInfoRightLine(FontAwesome.fa_map_marker, user.Location);
            tryAddInfoRightLine(FontAwesome.fa_heart_o, user.Intrerests);
            tryAddInfoRightLine(FontAwesome.fa_suitcase, user.Occupation);
            infoTextRight.NewParagraph();
            if (!string.IsNullOrEmpty(user.Twitter))
                tryAddInfoRightLine(FontAwesome.fa_twitter, "@" + user.Twitter);
            tryAddInfoRightLine(FontAwesome.fa_globe, user.Website);
            tryAddInfoRightLine(FontAwesome.fa_skype, user.Skype);

            if (user.Statistics != null)
            {
                levelBadge.Show();
                levelText.Text = user.Statistics.Level.Current.ToString();

                scoreText.Add(createScoreText("Ranked Score"));
                scoreNumberText.Add(createScoreNumberText(user.Statistics.RankedScore.ToString(@"#,0")));
                scoreText.Add(createScoreText("Accuracy"));
                scoreNumberText.Add(createScoreNumberText($"{user.Statistics.Accuracy}%"));
                scoreText.Add(createScoreText("Play Count"));
                scoreNumberText.Add(createScoreNumberText(user.Statistics.PlayCount.ToString(@"#,0")));
                scoreText.Add(createScoreText("Total Score"));
                scoreNumberText.Add(createScoreNumberText(user.Statistics.TotalScore.ToString(@"#,0")));
                scoreText.Add(createScoreText("Total Hits"));
                scoreNumberText.Add(createScoreNumberText(user.Statistics.TotalHits.ToString(@"#,0")));
                scoreText.Add(createScoreText("Max Combo"));
                scoreNumberText.Add(createScoreNumberText(user.Statistics.MaxCombo.ToString(@"#,0")));
                scoreText.Add(createScoreText("Replay Watched by Others"));
                scoreNumberText.Add(createScoreNumberText(user.Statistics.ReplayWatched.ToString(@"#,0")));

                gradeSS.Count = user.Statistics.GradesCount.SS;
                gradeSS.Show();
                gradeS.Count = user.Statistics.GradesCount.S;
                gradeS.Show();
                gradeA.Count = user.Statistics.GradesCount.A;
                gradeA.Show();

                chartContainer.Add(new RankChart(user) { RelativeSizeAxes = Axes.Both });
            }
        }

        // These could be local functions when C# 7 enabled

        private OsuSpriteText createScoreText(string text) => new OsuSpriteText
        {
            TextSize = 14,
            Text = text
        };

        private OsuSpriteText createScoreNumberText(string text) => new OsuSpriteText
        {
            TextSize = 14,
            Font = @"Exo2.0-Bold",
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight,
            Text = text
        };

        private void tryAddInfoRightLine(FontAwesome icon, string str)
        {
            if (string.IsNullOrEmpty(str)) return;
            infoTextRight.AddTextAwesome(icon);
            infoTextRight.AddText(" " + str);
            infoTextRight.NewLine();
        }

        private class GradeBadge : Container
        {
            private const float width = 50;
            private readonly string grade;
            private readonly Sprite badge;
            private readonly SpriteText numberText;

            public int Count
            {
                set
                {
                    numberText.Text = value.ToString(@"#,0");
                }
            }

            public GradeBadge(string grade)
            {
                this.grade = grade;
                Width = width;
                Height = 41;
                Add(badge = new Sprite
                {
                    Width = width,
                    Height = 26
                });
                Add(numberText = new SpriteText
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    TextSize = 14,
                    Font = @"Exo2.0-Bold"
                });
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                badge.Texture = textures.Get($"Grades/{grade}");
            }
        }
    }
}
