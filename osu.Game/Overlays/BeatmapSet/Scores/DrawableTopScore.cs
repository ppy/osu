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
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays.Profile.Sections.Ranks;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;
using System.Collections.Generic;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class DrawableTopScore : Container
    {
        private const float fade_duration = 100;
        private const float height = 100;
        private const float avatar_size = 80;
        private const float margin = 10;

        private readonly Box background;
        private readonly UpdateableAvatar avatar;
        private readonly DrawableFlag flag;
        private readonly ClickableTopScoreUsername username;
        private readonly SpriteText rankText;
        private readonly SpriteText date;
        private readonly DrawableRank rank;

        private readonly AutoSizeInfoColumn totalScore;
        private readonly MediumInfoColumn accuracy;
        private readonly MediumInfoColumn maxCombo;

        private readonly SmallInfoColumn hitGreat;
        private readonly SmallInfoColumn hitGood;
        private readonly SmallInfoColumn hitMeh;
        private readonly SmallInfoColumn hitMiss;
        private readonly SmallInfoColumn pp;

        private readonly ModsInfoColumn modsInfo;

        private APIScoreInfo score;
        public APIScoreInfo Score
        {
            get { return score; }
            set
            {
                if (score == value) return;
                score = value;

                avatar.User = username.User = score.User;
                flag.Country = score.User.Country;
                date.Text = $@"achieved {score.Date.Humanize()}";
                rank.UpdateRank(score.Rank);

                totalScore.Value = $@"{score.TotalScore:N0}";
                accuracy.Value = $@"{score.Accuracy:P2}";
                maxCombo.Value = $@"{score.MaxCombo:N0}x";

                hitGreat.Value = $"{score.Statistics[HitResult.Great]}";
                hitGood.Value = $"{score.Statistics[HitResult.Good]}";
                hitMeh.Value = $"{score.Statistics[HitResult.Meh]}";
                hitMiss.Value = $"{score.Statistics[HitResult.Miss]}";
                pp.Value = $@"{score.PP:N0}";

                modsInfo.ClearMods();
                modsInfo.Mods = score.Mods;
            }
        }

        public DrawableTopScore()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            CornerRadius = 3;
            Masking = true;
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
                    Colour = Color4.White,
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
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 2),
                                    Children = new Drawable[]
                                    {
                                        rankText = new SpriteText
                                        {
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            Text = "#1",
                                            TextSize = 20,
                                            Font = @"Exo2.0-BoldItalic",
                                        },
                                        rank = new DrawableRank(ScoreRank.F)
                                        {
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            Size = new Vector2(30),
                                            FillMode = FillMode.Fit,
                                        },
                                    }
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
                                            TextSize = 20,
                                        },
                                        date = new SpriteText
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            TextSize = 10,
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
                            Width = 0.7f,
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
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(margin, 0),
                                        Children = new Drawable[]
                                        {
                                            hitGreat = new SmallInfoColumn("300", 20),
                                            hitGood = new SmallInfoColumn("100", 20),
                                            hitMeh = new SmallInfoColumn("50", 20),
                                            hitMiss = new SmallInfoColumn("miss", 20),
                                            pp = new SmallInfoColumn("pp", 20),
                                            modsInfo = new ModsInfoColumn("mods"),
                                        }
                                    },
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(margin, 0),
                                        Children = new Drawable[]
                                        {
                                            totalScore = new AutoSizeInfoColumn("Total Score"),
                                            accuracy = new MediumInfoColumn("Accuracy"),
                                            maxCombo = new MediumInfoColumn("Max Combo"),
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
            date.Colour = rankText.Colour = colours.ContextMenuGray;
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(Color4.WhiteSmoke, fade_duration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeColour(Color4.White, fade_duration, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        private class ClickableTopScoreUsername : ClickableUserContainer
        {
            private const float fade_duration = 500;

            private readonly Box underscore;
            private readonly Container underscoreContainer;
            private readonly SpriteText text;

            private Color4 hoverColour;

            public float TextSize
            {
                set
                {
                    if (text.TextSize == value) return;
                    text.TextSize = value;
                }
                get { return text.TextSize; }
            }

            public ClickableTopScoreUsername()
            {
                Add(underscoreContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                    Child = underscore = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                    }
                });
                Add(text = new SpriteText
                {
                    Font = @"Exo2.0-BoldItalic",
                    Colour = Color4.Black,
                });
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                hoverColour = underscore.Colour = colours.Blue;
                underscoreContainer.Position = new Vector2(0, TextSize / 2 - 1);
            }

            protected override void OnUserChange(User user)
            {
                text.Text = user.Username;
            }

            protected override bool OnHover(HoverEvent e)
            {
                text.FadeColour(hoverColour, fade_duration, Easing.OutQuint);
                underscore.FadeIn(fade_duration, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                text.FadeColour(Color4.Black, fade_duration, Easing.OutQuint);
                underscore.FadeOut(fade_duration, Easing.OutQuint);
                base.OnHoverLost(e);
            }
        }

        private class DrawableInfoColumn : FillFlowContainer
        {
            private readonly SpriteText headerText;
            private const float header_text_size = 12;

            public DrawableInfoColumn(string header)
            {
                AutoSizeAxes = Axes.Y;
                Direction = FillDirection.Vertical;
                Spacing = new Vector2(0, 2);
                Children = new Drawable[]
                {
                    new Container
                    {
                        AutoSizeAxes = Axes.X,
                        Height = header_text_size,
                        Child = headerText = new SpriteText
                        {
                            TextSize = 12,
                            Text = header.ToUpper(),
                        }
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 3,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.LightGray,
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                headerText.Colour = colours.ContextMenuGray;
            }
        }

        private class ModsInfoColumn : DrawableInfoColumn
        {
            private readonly FillFlowContainer modsContainer;

            public IEnumerable<Mod> Mods
            {
                set
                {
                    foreach (Mod mod in value)
                        modsContainer.Add(new ModIcon(mod)
                        {
                            AutoSizeAxes = Axes.Both,
                            Scale = new Vector2(0.3f),
                        });
                }
            }

            public ModsInfoColumn(string header) : base(header)
            {
                AutoSizeAxes = Axes.Both;
                Add(modsContainer = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                });
            }

            public void ClearMods() => modsContainer.Clear();
        }

        private class TextInfoColumn : DrawableInfoColumn
        {
            private readonly SpriteText valueText;

            public string Value
            {
                set
                {
                    if (valueText.Text == value)
                        return;
                    valueText.Text = value;
                }
                get { return valueText.Text; }
            }

            public TextInfoColumn(string header, float valueTextSize = 25) : base(header)
            {
                Add(valueText = new SpriteText
                {
                    TextSize = valueTextSize,
                    Font = @"Exo2.0-Light",
                });
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                valueText.Colour = colours.ContextMenuGray;
            }
        }

        private class AutoSizeInfoColumn : TextInfoColumn
        {
            public AutoSizeInfoColumn(string header, float valueTextSize = 25) : base(header, valueTextSize)
            {
                AutoSizeAxes = Axes.Both;
            }
        }

        private class MediumInfoColumn : TextInfoColumn
        {
            private const float width = 70;

            public MediumInfoColumn(string header, float valueTextSize = 25) : base(header, valueTextSize)
            {
                Width = width;
            }
        }

        private class SmallInfoColumn : TextInfoColumn
        {
            private const float width = 40;

            public SmallInfoColumn(string header, float valueTextSize = 25) : base(header, valueTextSize)
            {
                Width = width;
            }
        }
    }
}
