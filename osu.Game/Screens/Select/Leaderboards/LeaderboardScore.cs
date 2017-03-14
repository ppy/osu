// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Modes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Allocation;
using System.Linq;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class LeaderboardScore : Container
    {
        private const float height = 70;
        private const float corner_radius = 5;
        private const float edge_margin = 10;
        private const float background_alpha = 0.25f;
        private const float score_rank_size = 40f;

        private readonly EdgeEffect imageShadow = new EdgeEffect
        {
            Type = EdgeEffectType.Shadow,
            Radius = 1,
            Colour = Color4.Black.Opacity(0.2f),
        };

        private Box background;
        private Container content, avatarContainer;
        private Sprite scoreRank;
        private OsuSpriteText nameLabel;
        private GlowingSpriteText scoreLabel;
        private ScoreComponentLabel maxCombo, accuracy;
        private Container flagBadgeContainer;
        private FillFlowContainer<ScoreModIcon> modsContainer;

        private readonly int index;
        public readonly Score Score;

        protected override bool OnHover(Framework.Input.InputState state)
        {
            background.FadeTo(0.5f, 300, EasingTypes.OutQuint);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(Framework.Input.InputState state)
        {
            background.FadeTo(background_alpha, 200, EasingTypes.OutQuint);
            base.OnHoverLost(state);
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            scoreRank.Texture = textures.Get($@"Badges/ScoreRanks/{Score.Rank.GetDescription()}");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // TODO: This fade to 0.01 is hacky, find a better way
            FadeTo(0.01f);

            foreach (Drawable d in new Drawable[] { avatarContainer, nameLabel, scoreLabel, scoreRank, flagBadgeContainer, maxCombo, accuracy, modsContainer, })
            {
                d.FadeOut();
            }

            content.MoveToY(75);
            avatarContainer.MoveToX(75);
            nameLabel.MoveToX(150);

            Delay(index * 50);
            Schedule(() =>
            {
                FadeIn(200);
                content.MoveToY(0, 800, EasingTypes.OutQuint);

                Delay(100);
                Schedule(() =>
                {
                    avatarContainer.FadeIn(300, EasingTypes.OutQuint);
                    nameLabel.FadeIn(350, EasingTypes.OutQuint);

                    avatarContainer.MoveToX(0, 300, EasingTypes.OutQuint);
                    nameLabel.MoveToX(0, 350, EasingTypes.OutQuint);

                    Delay(250);
                    Schedule(() =>
                    {
                        scoreLabel.FadeIn(200);
                        scoreRank.FadeIn(200);

                        Delay(50);
                        Schedule(() =>
                        {
                            var drawables = new Drawable[] { flagBadgeContainer, maxCombo, accuracy, modsContainer, };

                            for (int i = 0; i < drawables.Length; i++)
                            {
                                drawables[i].FadeIn(100 + (i * 50));
                            }
                        });
                    });
                });
            });
        }

        public LeaderboardScore(Score score, int i)
        {
            Score = score;
            index = i;

            RelativeSizeAxes = Axes.X;
            Height = height;

            var flag = Score.User.Region.CreateDrawable();
            flag.Width = 30;
            flag.RelativeSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 40,
                    Children = new[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = @"Exo2.0-MediumItalic",
                            TextSize = 22,
                            Text = index.ToString(),
                        },
                    },
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = 40, },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            CornerRadius = corner_radius,
                            Masking = true,
                            Children = new[]
                            {
                                background = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black,
                                    Alpha = background_alpha,
                                },
                            },
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(edge_margin),
                            Children = new Drawable[]
                            {
                                avatarContainer = new Container
                                {
                                    Size = new Vector2(height - edge_margin * 2, height - edge_margin * 2),
                                    CornerRadius = corner_radius,
                                    Masking = true,
                                    EdgeEffect = imageShadow,
                                    Children = new Drawable[]
                                    {
                                        new Overlays.Toolbar.ToolbarUserButton.Avatar
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            UserId = Score.User.Id,
                                            Masking = false,
                                        },
                                    },
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    AutoSizeAxes = Axes.X,
                                    Position = new Vector2(height - edge_margin, 0f),
                                    Children = new Drawable[]
                                    {
                                        nameLabel = new OsuSpriteText
                                        {
                                            Text = Score.User?.Username,
                                            Font = @"Exo2.0-BoldItalic",
                                            TextSize = 23,
                                        },
                                        new FillFlowContainer
                                        {
                                            Origin = Anchor.BottomLeft,
                                            Anchor = Anchor.BottomLeft,
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Spacing = new Vector2(10f, 0f),
                                            Children = new Drawable[]
                                            {
                                                flagBadgeContainer = new Container
                                                {
                                                    Size = new Vector2(87f, 20f),
                                                    Masking = true,
                                                    Children = new Drawable[]
                                                    {
                                                        flag,
                                                    },
                                                },
                                                new FillFlowContainer
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Horizontal,
                                                    Spacing = new Vector2(10f, 0f),
                                                    Margin = new MarginPadding { Left = 10, },
                                                    Children = new Drawable[]
                                                    {
                                                        maxCombo = new ScoreComponentLabel(FontAwesome.fa_circle_o, Score.MaxCombo.ToString()),
                                                        accuracy = new ScoreComponentLabel(FontAwesome.fa_circle_o, Score.Accuracy.ToString()),
                                                    },
                                                },
                                            },
                                        },
                                    },
                                },
                                scoreRank = new Sprite
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Size = new Vector2(score_rank_size),
                                    Position = new Vector2(0f, -10f),
                                },
                                scoreLabel = new GlowingSpriteText(string.Format("{0:n0}", Score.TotalScore), @"Venera", 23, Color4.White, OsuColour.FromHex(@"83ccfa"))
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Position = new Vector2(-score_rank_size - 5f, 0f),
                                },
                                modsContainer = new FillFlowContainer<ScoreModIcon>
                                {
                                    Anchor = Anchor.BottomRight,
                                    Origin = Anchor.BottomRight,
                                    AutoSizeAxes = Axes.Both,
                                    // TODO: Probably remove? Seems like others don't like this kind of thing
                                    Position = new Vector2(0f, 4f), //properly align the mod icons
                                    Direction = FillDirection.Horizontal,
                                },
                            },
                        },
                    },
                },
            };

            foreach (Mod mod in Score.Mods)
            {
                // TODO: Get actual mod colours
                modsContainer.Add(new ScoreModIcon(mod.Icon, OsuColour.FromHex(@"ffcc22")));
            }
        }

        class GlowingSpriteText : Container
        {
            public GlowingSpriteText(string text, string font, int textSize, Color4 textColour, Color4 glowColour)
            {
                AutoSizeAxes = Axes.Both;

                Children = new Drawable[]
                {
                    new BufferedContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        BlurSigma = new Vector2(4),
                        CacheDrawnFrameBuffer = true,
                        RelativeSizeAxes = Axes.Both,
                        BlendingMode = BlendingMode.Additive,
                        Size = new Vector2(3f),
                        Children = new[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Font = font,
                                TextSize = textSize,
                                Text = text,
                                Colour = glowColour,
                                Shadow = false,
                            },
                        },
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = font,
                        TextSize = textSize,
                        Text = text,
                        Colour = textColour,
                        Shadow = false,
                    },
                };
            }
        }

        class ScoreModIcon : Container
        {
            public ScoreModIcon(FontAwesome icon, Color4 colour)
            {
                AutoSizeAxes = Axes.Both;

                Children = new[]
                {
                    new TextAwesome
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Icon = FontAwesome.fa_osu_mod_bg,
                        Colour = colour,
                        Shadow = true,
                        TextSize = 30,
                    },
                    new TextAwesome
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Icon = icon,
                        Colour = OsuColour.Gray(84),
                        TextSize = 18,
                    },
                };
            }
        }

        class ScoreComponentLabel : Container
        {
            public ScoreComponentLabel(FontAwesome icon, string value)
            {
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;
                Size = new Vector2(60f, 20f);
                Padding = new MarginPadding { Top = 10f, };

                Children = new Drawable[]
                {
                    new TextAwesome
                    {
                        Origin = Anchor.Centre,
                        Icon = FontAwesome.fa_square,
                        Colour = OsuColour.FromHex(@"3087ac"),
                        Rotation = 45,
                        Shadow = true,
                    },
                    new TextAwesome
                    {
                        Origin = Anchor.Centre,
                        Icon = icon,
                        Colour = OsuColour.FromHex(@"a4edff"),
                        Scale = new Vector2(0.8f),
                    },
                    new GlowingSpriteText(value, @"Exo2.0-Bold", 17, Color4.White, OsuColour.FromHex(@"83ccfa"))
                    {
                        Origin = Anchor.CentreLeft,
                        Margin = new MarginPadding { Left = 15, },
                    },
                };
            }
        }
    }
}
