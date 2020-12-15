// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public class GameplayLeaderboardScore : CompositeDrawable
    {
        private readonly OsuSpriteText positionText, positionSymbol, userString;
        private readonly GlowingSpriteText scoreText;

        public Action OnScoreChange;

        private int? scorePosition;

        public int? ScorePosition
        {
            get => scorePosition;
            set
            {
                scorePosition = value;

                if (scorePosition.HasValue)
                    positionText.Text = $"#{scorePosition.Value.ToMetric(decimals: scorePosition < 100000 ? 1 : 0)}";

                positionText.FadeTo(scorePosition.HasValue ? 1 : 0);
                positionSymbol.FadeTo(scorePosition.HasValue ? 1 : 0);
            }
        }

        private double totalScore;

        public double TotalScore
        {
            get => totalScore;
            set
            {
                totalScore = value;
                scoreText.Text = totalScore.ToString("N0");

                OnScoreChange?.Invoke();
            }
        }

        private User user;

        public User User
        {
            get => user;
            set
            {
                user = value;
                userString.Text = user?.Username;
            }
        }

        public GameplayLeaderboardScore()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new Container
            {
                Masking = true,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopRight,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Margin = new MarginPadding { Right = 2.5f },
                        Spacing = new Vector2(2.5f),
                        Children = new[]
                        {
                            positionText = new OsuSpriteText
                            {
                                Alpha = 0,
                                Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                            },
                            positionSymbol = new OsuSpriteText
                            {
                                Alpha = 0,
                                Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold),
                                Text = ">",
                            },
                        }
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopLeft,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Margin = new MarginPadding { Left = 2.5f },
                        Spacing = new Vector2(2.5f),
                        Children = new Drawable[]
                        {
                            userString = new OsuSpriteText
                            {
                                Size = new Vector2(80, 16),
                                Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold, italics: true),
                            },
                            scoreText = new GlowingSpriteText
                            {
                                GlowColour = Color4Extensions.FromHex(@"83ccfa"),
                                Font = OsuFont.Numeric.With(size: 14),
                            }
                        }
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            positionText.Colour = colours.YellowLight;
            positionSymbol.Colour = colours.Yellow;
        }
    }
}
