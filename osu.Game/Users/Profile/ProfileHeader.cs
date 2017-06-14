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

namespace osu.Game.Users.Profile
{
    public class ProfileHeader : Container
    {
        private readonly User user;

        private readonly OsuTextFlowContainer infoText;
        private readonly FillFlowContainer<SpriteText> scoreText, scoreNumberText;

        private readonly Sprite levelBadge;
        private readonly SpriteText levelText;

        private const float cover_height = 350, info_height = 150, avatar_size = 110, avatar_bottom_position = -20, level_position = 30, level_height = 60;
        public ProfileHeader(User user)
        {
            this.user = user;
            RelativeSizeAxes = Axes.X;
            Height = cover_height + info_height - UserProfile.TAB_HEIGHT;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = cover_height,
                    Children = new Drawable[]
                    {
                        new AsyncLoadWrapper(new UserCoverBackground(user)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            FillMode = FillMode.Fill,
                            OnLoadComplete = d => d.FadeInFromZero(200)
                        })
                        {
                            Masking = true,
                            RelativeSizeAxes = Axes.Both
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            ColourInfo = ColourInfo.GradientVertical(Color4.Black.Opacity(0.1f), Color4.Black.Opacity(0.75f))
                        },
                        new UpdateableAvatar
                        {
                            User = user,
                            Size = new Vector2(avatar_size),
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            X = UserProfile.CONTENT_X_MARGIN,
                            Y = avatar_bottom_position,
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
                            X = UserProfile.CONTENT_X_MARGIN + avatar_size + 10,
                            Y = avatar_bottom_position,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = user.Username,
                                    TextSize = 25,
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
                infoText = new OsuTextFlowContainer(t =>
                {
                    t.TextSize = 12;
                    t.Alpha = 0.8f;
                })
                {
                    Y = cover_height + 20,
                    Margin = new MarginPadding { Horizontal = UserProfile.CONTENT_X_MARGIN },
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    ParagraphSpacing = 1
                },
                new Container
                {
                    X = -UserProfile.CONTENT_X_MARGIN,
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
                                    Width = 50
                                },
                                levelText = new OsuSpriteText
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Y = 11,
                                    TextSize = 18
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
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Height = info_height - UserProfile.TAB_HEIGHT - 15,
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

            Action<SpriteText> bold = t =>
            {
                t.Font = @"Exo2.0-Bold";
                t.Alpha = 1;
            };
            // placeholder text
            infoText.AddTextAwesome(FontAwesome.fa_map_marker);
            infoText.AddText(" position     ");
            infoText.AddTextAwesome(FontAwesome.fa_twitter);
            infoText.AddText(" tweet     ");
            infoText.AddTextAwesome(FontAwesome.fa_heart_o);
            infoText.AddText(" favorite     ");
            infoText.NewParagraph();
            infoText.AddText("0 years old");
            infoText.NewLine();
            infoText.AddText("Commander of ");
            infoText.AddText("The Color Scribbles", bold);
            infoText.NewParagraph();
            infoText.AddText("Joined since ");
            infoText.AddText("June 2017", bold);
            infoText.NewLine();
            infoText.AddText("Last seen ");
            infoText.AddText("0 minutes ago", bold);
            infoText.NewParagraph();
            infoText.AddText("Play with ");
            infoText.AddText("Mouse, Keyboard, Tablet", bold);

            levelText.Text = "98";

            scoreText.Add(createScoreText("Ranked Score"));
            scoreNumberText.Add(createScoreNumberText("1,870,716,897"));
            scoreText.Add(createScoreText("Accuracy"));
            scoreNumberText.Add(createScoreNumberText("98.51%"));
            scoreText.Add(createScoreText("Play Count"));
            scoreNumberText.Add(createScoreNumberText("25,287"));
            scoreText.Add(createScoreText("Total Score"));
            scoreNumberText.Add(createScoreNumberText("28,444,797,570"));
            scoreText.Add(createScoreText("Total Hits"));
            scoreNumberText.Add(createScoreNumberText("4,612,765"));
            scoreText.Add(createScoreText("Max Combo"));
            scoreNumberText.Add(createScoreNumberText("2,056"));
            scoreText.Add(createScoreText("Replay Watched"));
            scoreNumberText.Add(createScoreNumberText("23"));
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            levelBadge.Texture = textures.Get(@"Profile/levelbadge");
        }

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
    }
}
