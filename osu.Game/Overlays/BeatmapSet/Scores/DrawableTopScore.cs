// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays.Profile.Sections.Ranks;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class DrawableTopScore : Container
    {
        private const float fade_duration = 100;
        private const float height = 200;
        private const float avatar_size = 80;
        private const float margin = 10;

        private readonly Box background;
        private readonly Box bottomBackground;
        private readonly Box middleLine;
        private readonly UpdateableAvatar avatar;
        private readonly DrawableFlag flag;
        private readonly ClickableUsername username;
        private readonly OsuSpriteText rankText;
        private readonly OsuSpriteText date;
        private readonly DrawableRank rank;
        private readonly InfoColumn totalScore;
        private readonly InfoColumn accuracy;
        private readonly InfoColumn statistics;
        private readonly ScoreModsContainer modsContainer;

        private ScoreInfo score;

        public ScoreInfo Score
        {
            get => score;
            set
            {
                if (score == value) return;

                score = value;

                avatar.User = username.User = score.User;
                flag.Country = score.User.Country;
                date.Text = $@"achieved {score.Date:MMM d, yyyy}";
                rank.UpdateRank(score.Rank);

                totalScore.Value = $@"{score.TotalScore:N0}";
                accuracy.Value = $@"{score.Accuracy:P2}";
                statistics.Value = $"{score.Statistics[HitResult.Great]}/{score.Statistics[HitResult.Good]}/{score.Statistics[HitResult.Meh]}";

                modsContainer.Clear();
                foreach (Mod mod in score.Mods)
                    modsContainer.Add(new ModIcon(mod)
                    {
                        AutoSizeAxes = Axes.Both,
                        Scale = new Vector2(0.45f),
                    });
            }
        }

        public DrawableTopScore()
        {
            RelativeSizeAxes = Axes.X;
            Height = height;
            CornerRadius = 5;
            BorderThickness = 4;
            Masking = true;
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    AlwaysPresent = true, //used for correct border representation
                },
                avatar = new UpdateableAvatar
                {
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
                    Margin = new MarginPadding { Top = margin, Left = margin }
                },
                flag = new DrawableFlag
                {
                    Size = new Vector2(30, 20),
                    Position = new Vector2(margin * 2 + avatar_size, height / 4),
                },
                username = new ClickableUsername
                {
                    Origin = Anchor.BottomLeft,
                    TextSize = 30,
                    Position = new Vector2(margin * 2 + avatar_size, height / 4),
                    Margin = new MarginPadding { Bottom = 4 }
                },
                rankText = new OsuSpriteText
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.BottomRight,
                    Text = "#1",
                    Font = OsuFont.GetFont(size: 40, weight: FontWeight.Bold, italics: true),
                    Y = height / 4,
                    Margin = new MarginPadding { Right = margin }
                },
                date = new OsuSpriteText
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Y = height / 4,
                    Margin = new MarginPadding { Right = margin }
                },
                new Container
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    Children = new Drawable[]
                    {
                        bottomBackground = new Box { RelativeSizeAxes = Axes.Both },
                        middleLine = new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 1,
                        },
                        rank = new DrawableRank(ScoreRank.F)
                        {
                            Origin = Anchor.BottomLeft,
                            Size = new Vector2(avatar_size, 40),
                            FillMode = FillMode.Fit,
                            Y = height / 4,
                            Margin = new MarginPadding { Left = margin }
                        },
                        new FillFlowContainer<InfoColumn>
                        {
                            Origin = Anchor.BottomLeft,
                            AutoSizeAxes = Axes.Both,
                            Position = new Vector2(height / 2, height / 4),
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(15, 0),
                            Children = new[]
                            {
                                totalScore = new InfoColumn("Score"),
                                accuracy = new InfoColumn("Accuracy"),
                                statistics = new InfoColumn("300/100/50"),
                            },
                        },
                        modsContainer = new ScoreModsContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            Width = 80,
                            Position = new Vector2(height / 2, height / 4),
                        }
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = bottomBackground.Colour = colours.Gray4;
            middleLine.Colour = colours.Gray2;
            date.Colour = colours.Gray9;
            BorderColour = rankText.Colour = colours.Yellow;
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeIn(fade_duration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeOut(fade_duration, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        private class InfoColumn : FillFlowContainer
        {
            private readonly OsuSpriteText headerText;
            private readonly OsuSpriteText valueText;

            public string Value
            {
                set
                {
                    if (valueText.Text == value)
                        return;

                    valueText.Text = value;
                }
                get => valueText.Text;
            }

            public InfoColumn(string header)
            {
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Vertical;
                Spacing = new Vector2(0, 3);
                Children = new Drawable[]
                {
                    headerText = new OsuSpriteText
                    {
                        Text = header,
                        Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold)
                    },
                    valueText = new OsuSpriteText { Font = OsuFont.GetFont(size: 25, weight: FontWeight.Regular, italics: true) }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                headerText.Colour = colours.Gray9;
            }
        }
    }
}
