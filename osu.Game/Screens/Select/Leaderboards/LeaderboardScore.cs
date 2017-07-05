﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Rulesets.Mods;
using osu.Game.Users;
using osu.Framework;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class LeaderboardScore : ClickableContainer, IStateful<Visibility>
    {
        public static readonly float HEIGHT = 60;

        public readonly int RankPosition;
        public readonly Score Score;

        private const float corner_radius = 5;
        private const float edge_margin = 5;
        private const float background_alpha = 0.25f;
        private const float rank_width = 30;

        private readonly Box background;
        private readonly Container content;
        private readonly Container avatar;
        private readonly DrawableRank scoreRank;
        private readonly OsuSpriteText nameLabel;
        private readonly GlowingSpriteText scoreLabel;
        private readonly ScoreComponentLabel maxCombo;
        private readonly ScoreComponentLabel accuracy;
        private readonly Container flagBadgeContainer;
        private readonly FillFlowContainer<ScoreModIcon> modsContainer;

        private Visibility state;
        public Visibility State
        {
            get { return state; }
            set
            {
                state = value;

                switch (state)
                {
                    case Visibility.Hidden:
                        foreach (var d in new Drawable[] { avatar, nameLabel, scoreLabel, scoreRank, flagBadgeContainer, maxCombo, accuracy, modsContainer })
                            d.FadeOut();

                        Alpha = 0;

                        content.MoveToY(75);
                        avatar.MoveToX(75);
                        nameLabel.MoveToX(150);
                        break;
                    case Visibility.Visible:
                        FadeIn(200);
                        content.MoveToY(0, 800, EasingTypes.OutQuint);

                        Delay(100, true);
                        avatar.FadeIn(300, EasingTypes.OutQuint);
                        nameLabel.FadeIn(350, EasingTypes.OutQuint);

                        avatar.MoveToX(0, 300, EasingTypes.OutQuint);
                        nameLabel.MoveToX(0, 350, EasingTypes.OutQuint);

                        Delay(250, true);
                        scoreLabel.FadeIn(200);
                        scoreRank.FadeIn(200);

                        Delay(50, true);
                        var drawables = new Drawable[] { flagBadgeContainer, maxCombo, accuracy, modsContainer, };

                        for (int i = 0; i < drawables.Length; i++)
                        {
                            drawables[i].FadeIn(100 + i * 50);
                        }

                        break;
                }
            }
        }

        public LeaderboardScore(Score score, int rank)
        {
            Score = score;
            RankPosition = rank;

            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = rank_width,
                    Children = new[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = @"Exo2.0-MediumItalic",
                            TextSize = 22,
                            Text = RankPosition.ToString(),
                        },
                    },
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = rank_width, },
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
                                avatar = new DelayedLoadWrapper(
                                    new Avatar(Score.User)
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        CornerRadius = corner_radius,
                                        Masking = true,
                                        OnLoadComplete = d => d.FadeInFromZero(200),
                                        EdgeEffect = new EdgeEffectParameters
                                        {
                                            Type = EdgeEffectType.Shadow,
                                            Radius = 1,
                                            Colour = Color4.Black.Opacity(0.2f),
                                        },
                                    })
                                {
                                    TimeBeforeLoad = 500,
                                    RelativeSizeAxes = Axes.None,
                                    Size = new Vector2(HEIGHT - edge_margin * 2, HEIGHT - edge_margin * 2),
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    AutoSizeAxes = Axes.X,
                                    Position = new Vector2(HEIGHT - edge_margin, 0f),
                                    Children = new Drawable[]
                                    {
                                        nameLabel = new OsuSpriteText
                                        {
                                            Text = Score.User.Username,
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
                                                        new DrawableFlag(Score.User?.Country?.FlagName ?? "__")
                                                        {
                                                            Width = 30,
                                                            RelativeSizeAxes = Axes.Y,
                                                        },
                                                    },
                                                },
                                                new FillFlowContainer
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Horizontal,
                                                    Spacing = new Vector2(10f, 0f),
                                                    Margin = new MarginPadding { Left = edge_margin, },
                                                    Children = new Drawable[]
                                                    {
                                                        maxCombo = new ScoreComponentLabel(FontAwesome.fa_link, Score.MaxCombo.ToString()),
                                                        accuracy = new ScoreComponentLabel(FontAwesome.fa_crosshairs, string.Format(Score.Accuracy % 1 == 0 ? @"{0:P0}" : @"{0:P2}", Score.Accuracy)),
                                                    },
                                                },
                                            },
                                        },
                                    },
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(5f, 0f),
                                    Children = new Drawable[]
                                    {
                                        scoreLabel = new GlowingSpriteText(Score.TotalScore.ToString(@"N0"), @"Venera", 23, Color4.White, OsuColour.FromHex(@"83ccfa")),
                                        new Container
                                        {
                                            Size = new Vector2(40f, 20f),
                                            Children = new[]
                                            {
                                                scoreRank = new DrawableRank(Score.Rank)
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Size = new Vector2(40f),
                                                },
                                            },
                                        },
                                    },
                                },
                                modsContainer = new FillFlowContainer<ScoreModIcon>
                                {
                                    Anchor = Anchor.BottomRight,
                                    Origin = Anchor.BottomRight,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                },
                            },
                        },
                    },
                },
            };

            if (Score.Mods != null)
            {
                foreach (Mod mod in Score.Mods)
                {
                    // TODO: Get actual mod colours
                    modsContainer.Add(new ScoreModIcon(mod.Icon, OsuColour.FromHex(@"ffcc22")));
                }
            }
        }

        public void ToggleVisibility() => State = State == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;

        public override void Hide() => State = Visibility.Hidden;
        public override void Show() => State = Visibility.Visible;

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

        private class GlowingSpriteText : Container
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
                                FixedWidth = true,
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
                        FixedWidth = true,
                        TextSize = textSize,
                        Text = text,
                        Colour = textColour,
                        Shadow = false,
                    },
                };
            }
        }

        private class ScoreModIcon : Container
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
                        UseFullGlyphHeight = false,
                    },
                    new TextAwesome
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Icon = icon,
                        Colour = OsuColour.Gray(84),
                        TextSize = 18,
                        Position = new Vector2(0f, 2f),
                        UseFullGlyphHeight = false,
                    },
                };
            }
        }

        private class ScoreComponentLabel : Container
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
