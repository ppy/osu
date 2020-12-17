// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Users;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public class GameplayLeaderboardScore : CompositeDrawable
    {
        private const float regular_width = 215f;
        private const float extended_width = 235f;

        private const float panel_height = 35f;

        private OsuSpriteText positionText, scoreText, accuracyText, comboText;
        public readonly BindableDouble TotalScore = new BindableDouble();
        public readonly BindableDouble Accuracy = new BindableDouble();
        public readonly BindableInt Combo = new BindableInt();

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
            }
        }

        public User User { get; }

        private readonly bool localUser;

        public GameplayLeaderboardScore(User user, bool localUser)
        {
            User = user;
            this.localUser = localUser;

            AutoSizeAxes = Axes.Both;

            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            const float panel_shear = 0.15f;
            const float shear_width = panel_height * panel_shear;

            Color4 panelColour, textColour;
            float panelWidth;

            if (localUser)
            {
                panelWidth = extended_width;
                panelColour = Color4Extensions.FromHex("7fcc33");
                textColour = Color4.White;
            }
            else if (api.Friends.Any(f => User.Equals(f)))
            {
                panelWidth = extended_width;
                panelColour = Color4Extensions.FromHex("ffd966");
                textColour = Color4Extensions.FromHex("2e576b");
            }
            else
            {
                panelWidth = regular_width;
                panelColour = Color4Extensions.FromHex("3399cc");
                textColour = Color4.White;
            }

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Masking = true,
                    CornerRadius = 5f,
                    Shear = new Vector2(panel_shear, 0f),
                    Size = new Vector2(panelWidth, panel_height),
                    Child = new Box
                    {
                        Alpha = 0.5f,
                        RelativeSizeAxes = Axes.Both,
                        Colour = panelColour,
                    }
                },
                new GridContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Size = new Vector2(regular_width, panel_height),
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 35f),
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 85f),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            positionText = new OsuSpriteText
                            {
                                Padding = new MarginPadding { Right = shear_width / 2 },
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Colour = textColour,
                                Font = OsuFont.Torus.With(size: 14, weight: FontWeight.Bold),
                                Shadow = false,
                            },
                            new Container
                            {
                                Padding = new MarginPadding { Horizontal = shear_width / 3 },
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new Container
                                    {
                                        Masking = true,
                                        CornerRadius = 5f,
                                        Shear = new Vector2(panel_shear, 0f),
                                        RelativeSizeAxes = Axes.Both,
                                        Children = new[]
                                        {
                                            new Box
                                            {
                                                Alpha = 0.5f,
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = panelColour,
                                            },
                                        }
                                    },
                                    new OsuSpriteText
                                    {
                                        Padding = new MarginPadding { Left = shear_width },
                                        RelativeSizeAxes = Axes.X,
                                        Width = 0.8f,
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Colour = textColour,
                                        Font = OsuFont.Torus.With(size: 14, weight: FontWeight.SemiBold),
                                        Text = User.Username,
                                        Truncate = true,
                                        Shadow = false,
                                    }
                                }
                            },
                            new Container
                            {
                                Padding = new MarginPadding { Top = 2f, Right = 17.5f, Bottom = 5f },
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Colour = textColour,
                                Children = new Drawable[]
                                {
                                    scoreText = new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopLeft,
                                        Origin = Anchor.TopLeft,
                                        Spacing = new Vector2(0.5f, 0f),
                                        Font = OsuFont.Torus.With(size: 16, weight: FontWeight.SemiBold),
                                        Shadow = false,
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Anchor = Anchor.BottomLeft,
                                        Origin = Anchor.BottomLeft,
                                        Children = new Drawable[]
                                        {
                                            accuracyText = new OsuSpriteText
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                Font = OsuFont.Torus.With(size: 12, weight: FontWeight.SemiBold),
                                                Shadow = false,
                                            },
                                            comboText = new OsuSpriteText
                                            {
                                                Anchor = Anchor.CentreRight,
                                                Origin = Anchor.CentreRight,
                                                Font = OsuFont.Torus.With(size: 12, weight: FontWeight.SemiBold),
                                                Shadow = false,
                                            },
                                        }
                                    }
                                },
                            }
                        }
                    }
                }
            };

            TotalScore.BindValueChanged(v => scoreText.Text = v.NewValue.ToString("N0"), true);
            Accuracy.BindValueChanged(v => accuracyText.Text = v.NewValue.FormatAccuracy(), true);
            Combo.BindValueChanged(v => comboText.Text = $"{v.NewValue}x", true);
        }
    }
}
