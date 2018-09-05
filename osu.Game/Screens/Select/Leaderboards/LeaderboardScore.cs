// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Users;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class LeaderboardScore : OsuClickableContainer
    {
        public static readonly float HEIGHT = 60;

        public readonly int RankPosition;
        public readonly Score Score;

        private const float corner_radius = 5;
        private const float edge_margin = 5;
        private const float background_alpha = 0.25f;
        private const float rank_width = 30;

        private Box background;
        private Container content;
        private Drawable avatar;
        private DrawableRank scoreRank;
        private OsuSpriteText nameLabel;
        private GlowingSpriteText scoreLabel;
        private ScoreComponentLabel maxCombo;
        private ScoreComponentLabel accuracy;
        private Container flagBadgeContainer;
        private FillFlowContainer<ModIcon> modsContainer;

        public LeaderboardScore(Score score, int rank)
        {
            Score = score;
            RankPosition = rank;

            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
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
                            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
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
                            Children = new[]
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
                                                    Origin = Anchor.BottomLeft,
                                                    Anchor = Anchor.BottomLeft,
                                                    Size = new Vector2(87f, 20f),
                                                    Masking = true,
                                                    Children = new Drawable[]
                                                    {
                                                        new DrawableFlag(Score.User?.Country)
                                                        {
                                                            Width = 30,
                                                            RelativeSizeAxes = Axes.Y,
                                                        },
                                                    },
                                                },
                                                new FillFlowContainer
                                                {
                                                    Origin = Anchor.BottomLeft,
                                                    Anchor = Anchor.BottomLeft,
                                                    AutoSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Horizontal,
                                                    Spacing = new Vector2(10f, 0f),
                                                    Margin = new MarginPadding { Left = edge_margin },
                                                    Children = new Drawable[]
                                                    {
                                                        maxCombo = new ScoreComponentLabel(FontAwesome.fa_link, Score.MaxCombo.ToString(), "Max Combo"),
                                                        accuracy = new ScoreComponentLabel(FontAwesome.fa_crosshairs, string.Format(Score.Accuracy % 1 == 0 ? @"{0:P0}" : @"{0:P2}", Score.Accuracy), "Accuracy"),
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
                                modsContainer = new FillFlowContainer<ModIcon>
                                {
                                    Anchor = Anchor.BottomRight,
                                    Origin = Anchor.BottomRight,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    ChildrenEnumerable = Score.Mods.Select(mod => new ModIcon(mod) { Scale = new Vector2(0.375f) })
                                },
                            },
                        },
                    },
                },
            };
        }

        public override void Show()
        {
            foreach (var d in new[] { avatar, nameLabel, scoreLabel, scoreRank, flagBadgeContainer, maxCombo, accuracy, modsContainer })
                d.FadeOut();

            Alpha = 0;

            content.MoveToY(75);
            avatar.MoveToX(75);
            nameLabel.MoveToX(150);

            this.FadeIn(200);
            content.MoveToY(0, 800, Easing.OutQuint);

            using (BeginDelayedSequence(100, true))
            {
                avatar.FadeIn(300, Easing.OutQuint);
                nameLabel.FadeIn(350, Easing.OutQuint);

                avatar.MoveToX(0, 300, Easing.OutQuint);
                nameLabel.MoveToX(0, 350, Easing.OutQuint);

                using (BeginDelayedSequence(250, true))
                {
                    scoreLabel.FadeIn(200);
                    scoreRank.FadeIn(200);

                    using (BeginDelayedSequence(50, true))
                    {
                        var drawables = new Drawable[] { flagBadgeContainer, maxCombo, accuracy, modsContainer, };
                        for (int i = 0; i < drawables.Length; i++)
                            drawables[i].FadeIn(100 + i * 50);
                    }
                }
            }
        }

        protected override bool OnHover(InputState state)
        {
            background.FadeTo(0.5f, 300, Easing.OutQuint);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            background.FadeTo(background_alpha, 200, Easing.OutQuint);
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
                        Blending = BlendingMode.Additive,
                        Size = new Vector2(3f),
                        Children = new[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Font = font,
                                FixedWidth = true,
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
                        FixedWidth = true,
                        TextSize = textSize,
                        Text = text,
                        Colour = textColour,
                        Shadow = false,
                    },
                };
            }
        }

        private class ScoreComponentLabel : Container, IHasTooltip
        {
            private const float icon_size = 20;

            private readonly string name;
            private readonly FillFlowContainer content;

            public override bool Contains(Vector2 screenSpacePos) => content.Contains(screenSpacePos);

            public string TooltipText => name;

            public ScoreComponentLabel(FontAwesome icon, string value, string name)
            {
                this.name = name;
                AutoSizeAxes = Axes.Y;
                Width = 60;

                Child = content = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            Children = new[]
                            {
                                new SpriteIcon
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(icon_size),
                                    Rotation = 45,
                                    Colour = OsuColour.FromHex(@"3087ac"),
                                    Icon = FontAwesome.fa_square,
                                    Shadow = true,
                                },
                                new SpriteIcon
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(icon_size - 6),
                                    Colour = OsuColour.FromHex(@"a4edff"),
                                    Icon = icon,
                                },
                            },
                        },
                        new GlowingSpriteText(value, @"Exo2.0-Bold", 17, Color4.White, OsuColour.FromHex(@"83ccfa"))
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                    },
                };
            }
        }
    }
}
