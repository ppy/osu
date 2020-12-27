// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public class GameplayLeaderboardScore : CompositeDrawable, ILeaderboardScore
    {
        public const float EXTENDED_WIDTH = 255f;

        private const float regular_width = 235f;

        public const float PANEL_HEIGHT = 35f;

        public const float SHEAR_WIDTH = PANEL_HEIGHT * panel_shear;

        private const float panel_shear = 0.15f;

        private OsuSpriteText positionText, scoreText, accuracyText, comboText, usernameText;

        public BindableDouble TotalScore { get; } = new BindableDouble();
        public BindableDouble Accuracy { get; } = new BindableDouble(1);
        public BindableInt Combo { get; } = new BindableInt();
        public BindableBool HasQuit { get; } = new BindableBool();

        private int? scorePosition;

        public int? ScorePosition
        {
            get => scorePosition;
            set
            {
                if (value == scorePosition)
                    return;

                scorePosition = value;

                if (scorePosition.HasValue)
                    positionText.Text = $"#{scorePosition.Value.FormatRank()}";

                positionText.FadeTo(scorePosition.HasValue ? 1 : 0);
                updateState();
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

            Size = new Vector2(EXTENDED_WIDTH, PANEL_HEIGHT);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Container avatarContainer;

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
                                Padding = new MarginPadding { Right = SHEAR_WIDTH / 2 },
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Colour = Color4.White,
                                Font = OsuFont.Torus.With(size: 14, weight: FontWeight.Bold),
                                Shadow = false,
                            },
                            new Container
                            {
                                Padding = new MarginPadding { Horizontal = SHEAR_WIDTH / 3 },
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
                                    new FillFlowContainer
                                    {
                                        Padding = new MarginPadding { Left = SHEAR_WIDTH },
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        RelativeSizeAxes = Axes.Both,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(4f, 0f),
                                        Children = new Drawable[]
                                        {
                                            avatarContainer = new CircularContainer
                                            {
                                                Masking = true,
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                Size = new Vector2(25f),
                                                Children = new Drawable[]
                                                {
                                                    new Box
                                                    {
                                                        Name = "Placeholder while avatar loads",
                                                        Alpha = 0.3f,
                                                        RelativeSizeAxes = Axes.Both,
                                                        Colour = colours.Gray4,
                                                    }
                                                }
                                            },
                                            usernameText = new OsuSpriteText
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Width = 0.6f,
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

            LoadComponentAsync(new DrawableAvatar(User), avatarContainer.Add);

            TotalScore.BindValueChanged(v => scoreText.Text = v.NewValue.ToString("N0"), true);
            Accuracy.BindValueChanged(v => accuracyText.Text = v.NewValue.FormatAccuracy(), true);
            Combo.BindValueChanged(v => comboText.Text = $"{v.NewValue}x", true);
            HasQuit.BindValueChanged(_ => updateState());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateState();
            FinishTransforms(true);
        }

        private const double panel_transition_duration = 500;

        private void updateState()
        {
            if (HasQuit.Value)
            {
                // we will probably want to display this in a better way once we have a design.
                // and also show states other than quit.
                mainFillContainer.ResizeWidthTo(regular_width, panel_transition_duration, Easing.OutElastic);
                panelColour = Color4.Gray;
                textColour = Color4.White;
                return;
            }

            if (scorePosition == 1)
            {
                mainFillContainer.ResizeWidthTo(EXTENDED_WIDTH, panel_transition_duration, Easing.OutElastic);
                panelColour = Color4Extensions.FromHex("7fcc33");
                textColour = Color4.White;
            }
            else if (trackedPlayer)
            {
                mainFillContainer.ResizeWidthTo(EXTENDED_WIDTH, panel_transition_duration, Easing.OutElastic);
                panelColour = Color4Extensions.FromHex("ffd966");
                textColour = Color4Extensions.FromHex("2e576b");
            }
            else
            {
                mainFillContainer.ResizeWidthTo(regular_width, panel_transition_duration, Easing.OutElastic);
                panelColour = Color4Extensions.FromHex("3399cc");
                textColour = Color4.White;
            }
        }

        private Color4 panelColour
        {
            set
            {
                mainFillContainer.FadeColour(value, panel_transition_duration, Easing.OutQuint);
                centralFill.FadeColour(value, panel_transition_duration, Easing.OutQuint);
            }
        }

        private const double text_transition_duration = 200;

        private Color4 textColour
        {
            set
            {
                scoreText.FadeColour(value, text_transition_duration, Easing.OutQuint);
                accuracyText.FadeColour(value, text_transition_duration, Easing.OutQuint);
                comboText.FadeColour(value, text_transition_duration, Easing.OutQuint);
                usernameText.FadeColour(value, text_transition_duration, Easing.OutQuint);
                positionText.FadeColour(value, text_transition_duration, Easing.OutQuint);
            }
        }
    }
}
