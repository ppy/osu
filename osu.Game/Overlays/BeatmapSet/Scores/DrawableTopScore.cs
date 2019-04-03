// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class DrawableTopScore : Container
    {
        private const float fade_duration = 100;
        private const float avatar_size = 80;
        private const float margin = 10;

        private OsuColour colours;

        private Color4 backgroundIdleColour => colours.Gray3;
        private Color4 backgroundHoveredColour => colours.Gray4;

        private readonly FontUsage smallStatisticsFont = OsuFont.GetFont(size: 20);
        private readonly FontUsage largeStatisticsFont = OsuFont.GetFont(size: 25);

        private readonly Box background;
        private readonly UpdateableAvatar avatar;
        private readonly DrawableFlag flag;
        private readonly ClickableTopScoreUsername username;
        private readonly SpriteText rankText;
        private readonly SpriteText date;
        private readonly DrawableRank rank;

        private readonly FillFlowContainer<InfoColumn> statisticsContainer;

        private readonly TextColumn totalScoreColumn;
        private readonly TextColumn accuracyColumn;
        private readonly TextColumn maxComboColumn;
        private readonly TextColumn ppColumn;

        private readonly ModsInfoColumn modsInfo;

        private ScoreInfo score;

        public ScoreInfo Score
        {
            get => score;
            set
            {
                if (score == value)
                    return;

                score = value;

                avatar.User = username.User = score.User;
                flag.Country = score.User.Country;
                date.Text = $@"achieved {score.Date.Humanize()}";
                rank.UpdateRank(score.Rank);

                totalScoreColumn.Text = $@"{score.TotalScore:N0}";
                accuracyColumn.Text = $@"{score.Accuracy:P2}";
                maxComboColumn.Text = $@"{score.MaxCombo:N0}x";
                ppColumn.Text = $@"{score.PP:N0}";

                statisticsContainer.ChildrenEnumerable = score.Statistics.Select(kvp => new TextColumn(kvp.Key.GetDescription(), smallStatisticsFont) { Text = kvp.Value.ToString() });

                modsInfo.Mods = score.Mods;
            }
        }

        public DrawableTopScore()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 10;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(0.2f),
                Radius = 1,
                Offset = new Vector2(0, 1),
            };

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(margin),
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(margin, 0),
                            Children = new Drawable[]
                            {
                                rankText = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = "#1",
                                    Font = OsuFont.GetFont(size: 30, weight: FontWeight.Bold, italics: true)
                                },
                                rank = new DrawableRank(ScoreRank.F)
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(40),
                                    FillMode = FillMode.Fit,
                                },
                                avatar = new UpdateableAvatar
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(avatar_size),
                                    Masking = true,
                                    CornerRadius = 5,
                                    EdgeEffect = new EdgeEffectParameters
                                    {
                                        Type = EdgeEffectType.Shadow,
                                        Colour = Color4.Black.Opacity(0.25f),
                                        Offset = new Vector2(0, 2),
                                        Radius = 1,
                                    },
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 3),
                                    Children = new Drawable[]
                                    {
                                        username = new ClickableTopScoreUsername
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                        },
                                        date = new SpriteText
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Font = OsuFont.GetFont(size: 15, weight: FontWeight.Bold)
                                        },
                                        flag = new DrawableFlag
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Size = new Vector2(20, 13),
                                        },
                                    }
                                }
                            }
                        },
                        new Container
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Width = 0.65f,
                            Child = new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Spacing = new Vector2(margin),
                                Children = new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(margin, 0),
                                        Children = new Drawable[]
                                        {
                                            statisticsContainer = new FillFlowContainer<InfoColumn>
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Direction = FillDirection.Horizontal,
                                                Spacing = new Vector2(margin, 0),
                                            },
                                            ppColumn = new TextColumn("pp", smallStatisticsFont),
                                            modsInfo = new ModsInfoColumn(),
                                        }
                                    },
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(margin, 0),
                                        Children = new Drawable[]
                                        {
                                            totalScoreColumn = new TextColumn("total score", largeStatisticsFont),
                                            accuracyColumn = new TextColumn("accuracy", largeStatisticsFont),
                                            maxComboColumn = new TextColumn("max combo", largeStatisticsFont)
                                        }
                                    },
                                }
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            this.colours = colours;

            rankText.Colour = colours.Yellow;
            background.Colour = backgroundIdleColour;
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(backgroundHoveredColour, fade_duration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeColour(backgroundIdleColour, fade_duration, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        private class ClickableTopScoreUsername : ClickableUserContainer
        {
            private const float username_fade_duration = 150;

            private readonly FillFlowContainer hoverContainer;

            private readonly SpriteText normalText;
            private readonly SpriteText hoveredText;

            public ClickableTopScoreUsername()
            {
                var font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold, italics: true);

                Children = new Drawable[]
                {
                    normalText = new OsuSpriteText { Font = font },
                    hoverContainer = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Alpha = 0,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 1),
                        Children = new Drawable[]
                        {
                            hoveredText = new OsuSpriteText { Font = font },
                            new Box
                            {
                                BypassAutoSizeAxes = Axes.Both,
                                RelativeSizeAxes = Axes.X,
                                Height = 1
                            }
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                hoverContainer.Colour = colours.Blue;
            }

            protected override void OnUserChanged(User user) => normalText.Text = hoveredText.Text = user.Username;

            protected override bool OnHover(HoverEvent e)
            {
                hoverContainer.FadeIn(username_fade_duration, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                hoverContainer.FadeOut(username_fade_duration, Easing.OutQuint);
                base.OnHoverLost(e);
            }
        }

        private class InfoColumn : CompositeDrawable
        {
            private readonly Box separator;

            public InfoColumn(string title, Drawable content)
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 2),
                    Children = new[]
                    {
                        new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Black),
                            Text = title.ToUpper()
                        },
                        separator = new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 2
                        },
                        content
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                separator.Colour = colours.Gray5;
            }
        }

        private class TextColumn : InfoColumn
        {
            private readonly SpriteText text;

            public TextColumn(string title, FontUsage font)
                : this(title, new OsuSpriteText { Font = font })
            {
            }

            private TextColumn(string title, SpriteText text)
                : base(title, text)
            {
                this.text = text;
            }

            public LocalisedString Text
            {
                get => text.Text;
                set => text.Text = value;
            }
        }

        private class ModsInfoColumn : InfoColumn
        {
            private readonly FillFlowContainer modsContainer;

            public ModsInfoColumn()
                : this(new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal
                })
            {
            }

            private ModsInfoColumn(FillFlowContainer modsContainer)
                : base("mods", modsContainer)
            {
                this.modsContainer = modsContainer;
            }

            public IEnumerable<Mod> Mods
            {
                set
                {
                    modsContainer.Clear();

                    foreach (Mod mod in value)
                    {
                        modsContainer.Add(new ModIcon(mod)
                        {
                            AutoSizeAxes = Axes.Both,
                            Scale = new Vector2(0.3f),
                        });
                    }
                }
            }
        }
    }
}
