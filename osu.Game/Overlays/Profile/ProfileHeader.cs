// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Profile.Components;
using osu.Game.Overlays.Profile.Header;
using osu.Game.Users;
using Humanizer;

namespace osu.Game.Overlays.Profile
{
    public class ProfileHeader : Container
    {
        private readonly LinkFlowContainer infoTextLeft;
        private readonly LinkFlowContainer infoTextRight;
        private readonly FillFlowContainer<SpriteText> scoreText, scoreNumberText;
        private readonly RankGraph rankGraph;

        public readonly SupporterIcon SupporterTag;
        private readonly Container coverContainer;
        private readonly Sprite levelBadge;
        private readonly SpriteText levelText;
        private readonly GradeBadge gradeSSPlus, gradeSS, gradeSPlus, gradeS, gradeA;
        private readonly Box colourBar;
        private readonly DrawableFlag countryFlag;
        private readonly BadgeContainer badgeContainer;

        private const float cover_height = 350;
        private const float info_height = 150;
        private const float info_width = 220;
        private const float avatar_size = 110;
        private const float level_position = 30;
        private const float level_height = 60;
        private const float stats_width = 280;

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
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.1f), Color4.Black.Opacity(0.75f))
                        },
                        new Container
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Padding = new MarginPadding { Left = UserProfileOverlay.CONTENT_X_MARGIN, Bottom = 20, Right = stats_width + UserProfileOverlay.CONTENT_X_MARGIN },
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
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
                                    OpenOnClick = { Value = false },
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
                                        SupporterTag = new SupporterIcon
                                        {
                                            Alpha = 0,
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Y = -75,
                                            Size = new Vector2(25, 25)
                                        },
                                        new FillFlowContainer
                                        {
                                            Direction = FillDirection.Horizontal,
                                            AutoSizeAxes = Axes.Both,
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Y = -48,
                                            Children = new Drawable[]
                                            {
                                                usernameText = new OsuSpriteText
                                                {
                                                    Text = user.Username,
                                                    Font = OsuFont.GetFont(size: 30, weight: FontWeight.Regular, italics: true)
                                                },
                                                new ExternalLinkButton($@"https://osu.ppy.sh/users/{user.Id}")
                                                {
                                                    Anchor = Anchor.BottomLeft,
                                                    Origin = Anchor.BottomLeft,
                                                    Margin = new MarginPadding { Left = 3, Bottom = 3 }, //To better lineup with the font
                                                },
                                            }
                                        },
                                        countryFlag = new DrawableFlag(user.Country)
                                        {
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Width = 30,
                                            Height = 20
                                        }
                                    }
                                },
                                badgeContainer = new BadgeContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Origin = Anchor.BottomLeft,
                                    Margin = new MarginPadding { Bottom = 5 },
                                    Alpha = 0,
                                },
                            }
                        },
                        colourBar = new Box
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            X = UserProfileOverlay.CONTENT_X_MARGIN,
                            Height = 5,
                            Width = info_width,
                            Alpha = 0
                        }
                    }
                },
                new Box // this is a temporary workaround for incorrect masking behaviour of FillMode.Fill used in UserCoverBackground (see https://github.com/ppy/osu-framework/issues/1675)
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                    Y = cover_height,
                    Colour = OsuColour.Gray(34),
                },
                infoTextLeft = new LinkFlowContainer(t => t.Font = t.Font.With(size: 14))
                {
                    X = UserProfileOverlay.CONTENT_X_MARGIN,
                    Y = cover_height + 20,
                    Width = info_width,
                    AutoSizeAxes = Axes.Y,
                    ParagraphSpacing = 0.8f,
                    LineSpacing = 0.2f
                },
                infoTextRight = new LinkFlowContainer(t => t.Font = OsuFont.GetFont(size: 14, weight: FontWeight.Regular, italics: true))
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
                    Width = stats_width,
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
                                    Font = OsuFont.GetFont(size: 20)
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
                        new Container
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
                                },
                                rankGraph = new RankGraph
                                {
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

        private readonly OsuSpriteText usernameText;

        private User user;

        public User User
        {
            get => user;
            set
            {
                user = value;
                loadUser();
            }
        }

        private void loadUser()
        {
            LoadComponentAsync(new UserCoverBackground(user)
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fill,
                Depth = float.MaxValue,
            }, background =>
            {
                coverContainer.Add(background);
                background.FadeInFromZero(200);
            });

            if (user.IsSupporter)
                SupporterTag.Show();

            usernameText.Text = user.Username;

            if (!string.IsNullOrEmpty(user.Colour))
            {
                colourBar.Colour = OsuColour.FromHex(user.Colour);
                colourBar.Show();
            }

            void boldItalic(SpriteText t) => t.Font = t.Font.With(Typeface.Exo, weight: FontWeight.Bold, italics: true);
            void lightText(SpriteText t) => t.Alpha = 0.8f;

            OsuSpriteText createScoreText(string text) => new OsuSpriteText
            {
                Font = OsuFont.GetFont(size: 14),
                Text = text
            };

            OsuSpriteText createScoreNumberText(string text) => new OsuSpriteText
            {
                Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Text = text
            };

            if (user.Country != null)
            {
                infoTextLeft.AddText("From ", lightText);
                infoTextLeft.AddText(user.Country.FullName, boldItalic);
                countryFlag.Country = user.Country;
            }

            infoTextLeft.NewParagraph();

            if (user.JoinDate.ToUniversalTime().Year < 2008)
            {
                infoTextLeft.AddText(new DrawableJoinDate(user.JoinDate), lightText);
            }
            else
            {
                infoTextLeft.AddText("Joined ", lightText);
                infoTextLeft.AddText(new DrawableJoinDate(user.JoinDate), boldItalic);
            }

            if (user.LastVisit.HasValue)
            {
                infoTextLeft.NewLine();
                infoTextLeft.AddText("Last seen ", lightText);
                infoTextLeft.AddText(new DrawableDate(user.LastVisit.Value), boldItalic);
            }

            if (user.PlayStyle?.Length > 0)
            {
                infoTextLeft.NewParagraph();
                infoTextLeft.AddText("Plays with ", lightText);
                infoTextLeft.AddText(string.Join(", ", user.PlayStyle), boldItalic);
            }

            infoTextLeft.NewLine();
            infoTextLeft.AddText("Contributed ", lightText);
            infoTextLeft.AddLink("forum post".ToQuantity(user.PostCount), url: $"https://osu.ppy.sh/users/{user.Id}/posts", creationParameters: boldItalic);

            string websiteWithoutProtcol = user.Website;
            if (!string.IsNullOrEmpty(websiteWithoutProtcol))
            {
                int protocolIndex = websiteWithoutProtcol.IndexOf("//", StringComparison.Ordinal);
                if (protocolIndex >= 0)
                    websiteWithoutProtcol = websiteWithoutProtcol.Substring(protocolIndex + 2);
            }

            tryAddInfoRightLine(FontAwesome.Solid.MapMarker, user.Location);
            tryAddInfoRightLine(FontAwesome.Regular.Heart, user.Interests);
            tryAddInfoRightLine(FontAwesome.Solid.Suitcase, user.Occupation);
            infoTextRight.NewParagraph();
            if (!string.IsNullOrEmpty(user.Twitter))
                tryAddInfoRightLine(FontAwesome.Brands.Twitter, "@" + user.Twitter, $@"https://twitter.com/{user.Twitter}");
            tryAddInfoRightLine(FontAwesome.Solid.Gamepad, user.Discord);
            tryAddInfoRightLine(FontAwesome.Brands.Skype, user.Skype, @"skype:" + user.Skype + @"?chat");
            tryAddInfoRightLine(FontAwesome.Brands.Lastfm, user.Lastfm, $@"https://last.fm/users/{user.Lastfm}");
            tryAddInfoRightLine(FontAwesome.Solid.Globe, websiteWithoutProtcol, user.Website);

            if (user.Statistics != null)
            {
                levelBadge.Show();
                levelText.Text = user.Statistics.Level.Current.ToString();

                scoreText.Add(createScoreText("Ranked Score"));
                scoreNumberText.Add(createScoreNumberText(user.Statistics.RankedScore.ToString(@"#,0")));
                scoreText.Add(createScoreText("Accuracy"));
                scoreNumberText.Add(createScoreNumberText($"{user.Statistics.Accuracy:0.##}%"));
                scoreText.Add(createScoreText("Play Count"));
                scoreNumberText.Add(createScoreNumberText(user.Statistics.PlayCount.ToString(@"#,0")));
                scoreText.Add(createScoreText("Total Score"));
                scoreNumberText.Add(createScoreNumberText(user.Statistics.TotalScore.ToString(@"#,0")));
                scoreText.Add(createScoreText("Total Hits"));
                scoreNumberText.Add(createScoreNumberText(user.Statistics.TotalHits.ToString(@"#,0")));
                scoreText.Add(createScoreText("Max Combo"));
                scoreNumberText.Add(createScoreNumberText(user.Statistics.MaxCombo.ToString(@"#,0")));
                scoreText.Add(createScoreText("Replays Watched by Others"));
                scoreNumberText.Add(createScoreNumberText(user.Statistics.ReplaysWatched.ToString(@"#,0")));

                gradeSSPlus.DisplayCount = user.Statistics.GradesCount.SSPlus;
                gradeSSPlus.Show();
                gradeSS.DisplayCount = user.Statistics.GradesCount.SS;
                gradeSS.Show();
                gradeSPlus.DisplayCount = user.Statistics.GradesCount.SPlus;
                gradeSPlus.Show();
                gradeS.DisplayCount = user.Statistics.GradesCount.S;
                gradeS.Show();
                gradeA.DisplayCount = user.Statistics.GradesCount.A;
                gradeA.Show();

                rankGraph.User.Value = user;
            }

            badgeContainer.ShowBadges(user.Badges);
        }

        private void tryAddInfoRightLine(IconUsage icon, string str, string url = null)
        {
            if (string.IsNullOrEmpty(str)) return;

            infoTextRight.AddIcon(icon);
            if (url != null)
            {
                infoTextRight.AddLink(" " + str, url);
            }
            else
            {
                infoTextRight.AddText(" " + str);
            }

            infoTextRight.NewLine();
        }
    }
}
