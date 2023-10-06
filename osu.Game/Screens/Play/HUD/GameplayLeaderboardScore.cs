// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Rulesets.Scoring;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class GameplayLeaderboardScore : CompositeDrawable, ILeaderboardScore
    {
        public const float EXTENDED_WIDTH = regular_width + top_player_left_width_extension;

        private const float regular_width = 235f;

        // a bit hand-wavy, but there's a lot of hard-coded paddings in each of the grid's internals.
        private const float compact_width = 77.5f;

        private const float top_player_left_width_extension = 20f;

        public const float PANEL_HEIGHT = 35f;

        public const float SHEAR_WIDTH = PANEL_HEIGHT * panel_shear;

        private const float panel_shear = 0.15f;

        private const float rank_text_width = 35f;

        private const float avatar_size = 25f;

        private const double panel_transition_duration = 500;

        private const double text_transition_duration = 200;

        public Bindable<bool> Expanded = new Bindable<bool>();

        private OsuSpriteText positionText = null!, scoreText = null!, accuracyText = null!, comboText = null!, usernameText = null!;

        public BindableLong TotalScore { get; } = new BindableLong();
        public BindableDouble Accuracy { get; } = new BindableDouble(1);
        public BindableInt Combo { get; } = new BindableInt();
        public BindableBool HasQuit { get; } = new BindableBool();
        public Bindable<long> DisplayOrder { get; } = new Bindable<long>();

        private Func<ScoringMode, long>? getDisplayScoreFunction;

        public Func<ScoringMode, long> GetDisplayScore
        {
            set => getDisplayScoreFunction = value;
        }

        public Color4? BackgroundColour { get; set; }

        public Color4? TextColour { get; set; }

        private int? scorePosition;

        private bool scorePositionIsSet;

        public int? ScorePosition
        {
            get => scorePosition;
            set
            {
                // We always want to run once, as the incoming value may be null and require a visual update to "-".
                if (value == scorePosition && scorePositionIsSet)
                    return;

                scorePosition = value;

                positionText.Text = scorePosition.HasValue ? $"#{scorePosition.Value.FormatRank()}" : "-";
                scorePositionIsSet = true;

                updateState();
            }
        }

        public IUser? User { get; }

        /// <summary>
        /// Whether this score is the local user or a replay player (and should be focused / always visible).
        /// </summary>
        public readonly bool Tracked;

        private Container mainFillContainer = null!;

        private Box centralFill = null!;

        private Container backgroundPaddingAdjustContainer = null!;

        private GridContainer gridContainer = null!;

        private Container scoreComponents = null!;

        private IBindable<ScoringMode> scoreDisplayMode = null!;

        private bool isFriend;

        /// <summary>
        /// Creates a new <see cref="GameplayLeaderboardScore"/>.
        /// </summary>
        /// <param name="user">The score's player.</param>
        /// <param name="tracked">Whether the player is the local user or a replay player.</param>
        public GameplayLeaderboardScore(IUser? user, bool tracked)
        {
            User = user;
            Tracked = tracked;

            AutoSizeAxes = Axes.X;
            Height = PANEL_HEIGHT;

            GetDisplayScore = _ => TotalScore.Value;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OsuConfigManager osuConfigManager, IAPIProvider api)
        {
            Container avatarContainer;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    AutoSizeAxes = Axes.X,
                    RelativeSizeAxes = Axes.Y,
                    Margin = new MarginPadding { Left = top_player_left_width_extension },
                    Children = new Drawable[]
                    {
                        backgroundPaddingAdjustContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                mainFillContainer = new Container
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    RelativeSizeAxes = Axes.Both,
                                    Masking = true,
                                    CornerRadius = 5f,
                                    Shear = new Vector2(panel_shear, 0f),
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            Alpha = 0.5f,
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                    },
                                },
                            }
                        },
                        gridContainer = new GridContainer
                        {
                            RelativeSizeAxes = Axes.Y,
                            Width = compact_width, // will be updated by expanded state.
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            ColumnDimensions = new[]
                            {
                                new Dimension(GridSizeMode.Absolute, rank_text_width),
                                new Dimension(),
                                new Dimension(GridSizeMode.AutoSize),
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
                                                        Size = new Vector2(avatar_size),
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
                                                    usernameText = new TruncatingSpriteText
                                                    {
                                                        RelativeSizeAxes = Axes.X,
                                                        Width = 0.6f,
                                                        Anchor = Anchor.CentreLeft,
                                                        Origin = Anchor.CentreLeft,
                                                        Colour = Color4.White,
                                                        Font = OsuFont.Torus.With(size: 14, weight: FontWeight.SemiBold),
                                                        Text = User?.Username ?? string.Empty,
                                                        Shadow = false,
                                                    }
                                                }
                                            },
                                        }
                                    },
                                    scoreComponents = new Container
                                    {
                                        Padding = new MarginPadding { Top = 2f, Right = 17.5f, Bottom = 5f },
                                        AlwaysPresent = true, // required to smoothly animate autosize after hidden early.
                                        Masking = true,
                                        RelativeSizeAxes = Axes.Y,
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
                    }
                },
            };

            LoadComponentAsync(new DrawableAvatar(User), avatarContainer.Add);

            scoreDisplayMode = osuConfigManager.GetBindable<ScoringMode>(OsuSetting.ScoreDisplayMode);
            scoreDisplayMode.BindValueChanged(_ => updateScore());
            TotalScore.BindValueChanged(_ => updateScore(), true);

            Accuracy.BindValueChanged(v =>
            {
                accuracyText.Text = v.NewValue.FormatAccuracy();
                updateDetailsWidth();
            }, true);

            Combo.BindValueChanged(v =>
            {
                comboText.Text = $"{v.NewValue}x";
                updateDetailsWidth();
            }, true);

            HasQuit.BindValueChanged(_ => updateState());

            isFriend = User != null && api.Friends.Any(u => User.OnlineID == u.Id);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateState();
            Expanded.BindValueChanged(changeExpandedState, true);

            FinishTransforms(true);
        }

        private void updateScore() => scoreText.Text = (getDisplayScoreFunction?.Invoke(scoreDisplayMode.Value) ?? TotalScore.Value).ToString("N0");

        private void changeExpandedState(ValueChangedEvent<bool> expanded)
        {
            if (expanded.NewValue)
            {
                gridContainer.ResizeWidthTo(regular_width, panel_transition_duration, Easing.OutQuint);

                scoreComponents.FadeIn(panel_transition_duration, Easing.OutQuint);

                usernameText.FadeIn(panel_transition_duration, Easing.OutQuint);
            }
            else
            {
                gridContainer.ResizeWidthTo(compact_width, panel_transition_duration, Easing.OutQuint);

                scoreComponents.FadeOut(text_transition_duration, Easing.OutQuint);

                usernameText.FadeOut(text_transition_duration, Easing.OutQuint);
            }

            updateDetailsWidth();
        }

        private float? scoreComponentsTargetWidth;

        private void updateDetailsWidth()
        {
            const float score_components_min_width = 88f;

            float newWidth = Expanded.Value
                ? Math.Max(score_components_min_width, comboText.DrawWidth + accuracyText.DrawWidth + 25)
                : 0;

            if (scoreComponentsTargetWidth == newWidth)
                return;

            scoreComponentsTargetWidth = newWidth;
            scoreComponents.ResizeWidthTo(newWidth, panel_transition_duration, Easing.OutQuint);
        }

        private void updateState()
        {
            bool widthExtension = false;

            if (HasQuit.Value)
            {
                // we will probably want to display this in a better way once we have a design.
                // and also show states other than quit.
                panelColour = Color4.Gray;
                textColour = Color4.White;
                return;
            }

            if (scorePosition == 1)
            {
                widthExtension = true;
                panelColour = BackgroundColour ?? Color4Extensions.FromHex("7fcc33");
                textColour = TextColour ?? Color4.White;
            }
            else if (Tracked)
            {
                widthExtension = true;
                panelColour = BackgroundColour ?? Color4Extensions.FromHex("ffd966");
                textColour = TextColour ?? Color4Extensions.FromHex("2e576b");
            }
            else if (isFriend)
            {
                panelColour = BackgroundColour ?? Color4Extensions.FromHex("ff549a");
                textColour = TextColour ?? Color4.White;
            }
            else
            {
                panelColour = BackgroundColour ?? Color4Extensions.FromHex("3399cc");
                textColour = TextColour ?? Color4.White;
            }

            this.TransformTo(nameof(SizeContainerLeftPadding), widthExtension ? -top_player_left_width_extension : 0, panel_transition_duration, Easing.OutElastic);
        }

        public float SizeContainerLeftPadding
        {
            get => backgroundPaddingAdjustContainer.Padding.Left;
            set => backgroundPaddingAdjustContainer.Padding = new MarginPadding { Left = value };
        }

        private Color4 panelColour
        {
            set
            {
                mainFillContainer.FadeColour(value, panel_transition_duration, Easing.OutQuint);
                centralFill.FadeColour(value, panel_transition_duration, Easing.OutQuint);
            }
        }

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
