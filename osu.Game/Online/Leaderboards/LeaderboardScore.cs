// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Online.Leaderboards
{
    public class LeaderboardScore : OsuClickableContainer
    {
        public readonly int RankPosition;

        public const float HEIGHT = 60;

        private const float corner_radius = 5;
        private const float edge_margin = 5;
        private const float background_alpha = 0.25f;
        private const float rank_width = 30;

        protected Container RankContainer { get; private set; }

        private readonly ScoreInfo score;

        private Box background;
        private Container content;
        private Drawable avatar;
        private Drawable scoreRank;
        private OsuSpriteText nameLabel;
        private GlowingSpriteText scoreLabel;
        private Container flagBadgeContainer;
        private FillFlowContainer<ModIcon> modsContainer;

        private List<ScoreComponentLabel> statisticsLabels;

        public LeaderboardScore(ScoreInfo score, int rank)
        {
            this.score = score;
            RankPosition = rank;

            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var user = score.User;

            statisticsLabels = GetStatistics(score).Select(s => new ScoreComponentLabel(s)).ToList();

            Avatar innerAvatar;

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
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 22, italics: true),
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
                                    innerAvatar = new Avatar(user)
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        CornerRadius = corner_radius,
                                        Masking = true,
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
                                            Text = user.Username,
                                            Font = OsuFont.GetFont(size: 23, weight: FontWeight.Bold, italics: true)
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
                                                        new DrawableFlag(user.Country)
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
                                                    Children = statisticsLabels
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
                                        scoreLabel = new GlowingSpriteText(score.TotalScore.ToString(@"N0"), OsuFont.Numeric.With(size: 23), Color4.White, OsuColour.FromHex(@"83ccfa")),
                                        RankContainer = new Container
                                        {
                                            Size = new Vector2(40f, 20f),
                                            Children = new[]
                                            {
                                                scoreRank = new DrawableRank(score.Rank)
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Size = new Vector2(40f)
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
                                    ChildrenEnumerable = score.Mods.Select(mod => new ModIcon(mod) { Scale = new Vector2(0.375f) })
                                },
                            },
                        },
                    },
                },
            };

            innerAvatar.OnLoadComplete += d => d.FadeInFromZero(200);
        }

        public override void Show()
        {
            foreach (var d in new[] { avatar, nameLabel, scoreLabel, scoreRank, flagBadgeContainer, modsContainer }.Concat(statisticsLabels))
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
                        var drawables = new Drawable[] { flagBadgeContainer, modsContainer }.Concat(statisticsLabels).ToArray();
                        for (int i = 0; i < drawables.Length; i++)
                            drawables[i].FadeIn(100 + i * 50);
                    }
                }
            }
        }

        protected virtual IEnumerable<LeaderboardScoreStatistic> GetStatistics(ScoreInfo model) => new[]
        {
            new LeaderboardScoreStatistic(FontAwesome.Solid.Link, "Max Combo", model.MaxCombo.ToString()),
            new LeaderboardScoreStatistic(FontAwesome.Solid.Crosshairs, "Accuracy", string.Format(model.Accuracy % 1 == 0 ? @"{0:P0}" : @"{0:P2}", model.Accuracy))
        };

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeTo(0.5f, 300, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeTo(background_alpha, 200, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        private class GlowingSpriteText : Container
        {
            public GlowingSpriteText(string text, FontUsage font, Color4 textColour, Color4 glowColour)
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
                                Font = font.With(fixedWidth: true),
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
                        Font = font.With(fixedWidth: true),
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

            public ScoreComponentLabel(LeaderboardScoreStatistic statistic)
            {
                name = statistic.Name;
                AutoSizeAxes = Axes.Both;

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
                                    Icon = FontAwesome.Solid.Square,
                                    Shadow = true,
                                },
                                new SpriteIcon
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(icon_size - 6),
                                    Colour = OsuColour.FromHex(@"a4edff"),
                                    Icon = statistic.Icon,
                                },
                            },
                        },
                        new GlowingSpriteText(statistic.Value, OsuFont.GetFont(size: 17, weight: FontWeight.Bold), Color4.White, OsuColour.FromHex(@"83ccfa"))
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                    },
                };
            }
        }

        public class LeaderboardScoreStatistic
        {
            public IconUsage Icon;
            public string Value;
            public string Name;

            public LeaderboardScoreStatistic(IconUsage icon, string name, string value)
            {
                Icon = icon;
                Name = name;
                Value = value;
            }
        }
    }
}
