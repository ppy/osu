// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
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

        private OsuSpriteText positionText, scoreText, accuracyText, comboText, usernameText;

        public readonly BindableDouble TotalScore = new BindableDouble(1000000);
        public readonly BindableDouble Accuracy = new BindableDouble(1);
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
                updateColour();
            }
        }

        public User User { get; }

        private readonly bool trackedPlayer;

        private Container mainFillContainer;
        private Box centralFill;

        /// <summary>
        /// Creates a new <see cref="GameplayLeaderboardScore"/>.
        /// </summary>
        /// <param name="user">The score's player.</param>
        /// <param name="trackedPlayer">Whether the player is the local user or a replay player.</param>
        public GameplayLeaderboardScore(User user, bool trackedPlayer)
        {
            User = user;
            this.trackedPlayer = trackedPlayer;

            AutoSizeAxes = Axes.X;
            Height = panel_height;

            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateColour();
            FinishTransforms(true);
        }

        private void updateColour()
        {
            if (scorePosition == 1)
            {
                mainFillContainer.ResizeWidthTo(extended_width, 200, Easing.OutQuint);
                panelColour = Color4Extensions.FromHex("7fcc33");
                textColour = Color4.White;
            }
            else if (trackedPlayer)
            {
                mainFillContainer.ResizeWidthTo(extended_width, 200, Easing.OutQuint);
                panelColour = Color4Extensions.FromHex("ffd966");
                textColour = Color4Extensions.FromHex("2e576b");
            }
            else
            {
                mainFillContainer.ResizeWidthTo(regular_width, 200, Easing.OutQuint);
                panelColour = Color4Extensions.FromHex("3399cc");
                textColour = Color4.White;
            }
        }

        private Color4 panelColour
        {
            set
            {
                mainFillContainer.FadeColour(value, 200, Easing.OutQuint);
                centralFill.FadeColour(value, 200, Easing.OutQuint);
            }
        }

        private Color4 textColour
        {
            set
            {
                scoreText.FadeColour(value, 200, Easing.OutQuint);
                accuracyText.FadeColour(value, 200, Easing.OutQuint);
                comboText.FadeColour(value, 200, Easing.OutQuint);
                usernameText.FadeColour(value, 200, Easing.OutQuint);
                positionText.FadeColour(value, 200, Easing.OutQuint);
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            const float panel_shear = 0.15f;
            const float shear_width = panel_height * panel_shear;

            InternalChildren = new Drawable[]
            {
                mainFillContainer = new Container
                {
                    Width = regular_width,
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Masking = true,
                    CornerRadius = 5f,
                    Shear = new Vector2(panel_shear, 0f),
                    Child = new Box
                    {
                        Alpha = 0.5f,
                        RelativeSizeAxes = Axes.Both,
                    }
                },
                new GridContainer
                {
                    Width = regular_width,
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
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
                                Colour = Color4.White,
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
                                            centralFill = new Box
                                            {
                                                Alpha = 0.5f,
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = Color4Extensions.FromHex("3399cc"),
                                            },
                                        }
                                    },
                                    usernameText = new OsuSpriteText
                                    {
                                        Padding = new MarginPadding { Left = shear_width },
                                        RelativeSizeAxes = Axes.X,
                                        Width = 0.8f,
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Colour = Color4.White,
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
                                Colour = Color4.White,
                                Children = new Drawable[]
                                {
                                    scoreText = new OsuSpriteText
                                    {
                                        Spacing = new Vector2(-1f, 0f),
                                        Font = OsuFont.Torus.With(size: 16, weight: FontWeight.SemiBold, fixedWidth: true),
                                        Shadow = false,
                                    },
                                    accuracyText = new OsuSpriteText
                                    {
                                        Anchor = Anchor.BottomLeft,
                                        Origin = Anchor.BottomLeft,
                                        Font = OsuFont.Torus.With(size: 12, weight: FontWeight.SemiBold, fixedWidth: true),
                                        Spacing = new Vector2(-1f, 0f),
                                        Shadow = false,
                                    },
                                    comboText = new OsuSpriteText
                                    {
                                        Anchor = Anchor.BottomRight,
                                        Origin = Anchor.BottomRight,
                                        Spacing = new Vector2(-1f, 0f),
                                        Font = OsuFont.Torus.With(size: 12, weight: FontWeight.SemiBold, fixedWidth: true),
                                        Shadow = false,
                                    },
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
