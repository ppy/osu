// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Skinning;
using osuTK;
using Realms;

namespace osu.Game.Screens.RankingV2.Argon
{
    public partial class ArgonTotalScorePanel : CompositeDrawable, ISerialisableDrawable, IAnimatableSkinnable
    {
        public static readonly ColourInfo TEXT_GRADIENT = ColourInfo.GradientVertical(Colour4.White, Colour4.FromHex(@"B2E5FE"));

        private Container scoreContainer = null!;
        private Sprite perfectIndicator = null!;
        private Container personalBestIndicator = null!;
        private Box personalBestFlash = null!;
        private TotalScoreCounter totalScoreText = null!;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private Bindable<ScoringMode> scoringMode { get; } = new Bindable<ScoringMode>();

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, TextureStore textures, OsuConfigManager configManager)
        {
            Width = ArgonBeatmapInfoPanel.WIDTH * 1.3f;
            Height = 115;

            InternalChildren =
            [
                scoreContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Shear = OsuGame.SHEAR,
                    CornerRadius = ShearedButton.CORNER_RADIUS,
                    Masking = true,
                    EdgeEffect = ArgonBeatmapInfoPanel.CreateShadowEdgeEffect(),
                    Children =
                    [
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background5.Opacity(0.98f),
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Horizontal,
                            Padding = new MarginPadding
                            {
                                Vertical = 15,
                                Right = 30,
                            },
                            Spacing = new Vector2(20),
                            Shear = -OsuGame.SHEAR,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Children =
                            [
                                new Container
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Size = new Vector2(65),
                                    Child = perfectIndicator = new Sprite
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Texture = textures.Get(@"Icons/Ranking/perfect"),
                                        Size = new Vector2(75),
                                        Colour = new ColourInfo
                                        {
                                            TopLeft = Colour4.FromHex(@"00FFAA"),
                                            TopRight = Colour4.FromHex(@"7CF6FF"),
                                            BottomLeft = Colour4.FromHex(@"7CF6FF"),
                                            BottomRight = Colour4.FromHex(@"FF9AD7"),
                                        }
                                    },
                                },
                                totalScoreText = new TotalScoreCounter
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                },
                            ],
                        },
                    ],
                },
                personalBestIndicator = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    RelativeAnchorPosition = new Vector2(0.95f, 0),
                    Shear = OsuGame.SHEAR,
                    CornerRadius = ShearedButton.CORNER_RADIUS,
                    Masking = true,
                    EdgeEffect = ArgonBeatmapInfoPanel.CreateShadowEdgeEffect(),
                    Children =
                    [
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(Colour4.FromHex(@"#FFE7A8"), Colour4.FromHex(@"#FFB800")),
                        },
                        new OsuSpriteText
                        {
                            Colour = colourProvider.Background5,
                            Text = ResultsScreenStrings.PersonalBest.ToUpper(),
                            UseFullGlyphHeight = false,
                            Spacing = new Vector2(1.5f),
                            Font = OsuFont.Style.Body.With(weight: FontWeight.Bold),
                            Shear = -OsuGame.SHEAR,
                            Margin = new MarginPadding
                            {
                                Horizontal = 12,
                                Vertical = 6,
                            }
                        },
                        personalBestFlash = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.White,
                            Alpha = 0,
                            Blending = BlendingParameters.Additive,
                        },
                    ],
                },
            ];

            configManager.BindWith(OsuSetting.ScoreDisplayMode, scoringMode);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            score.BindValueChanged(_ => updateState());
            scoringMode.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            // keep the size of the total score text manageable when classic scoring mode is enabled
            // 7 digits is the assumed maximum for standardised - scale any longer score totals proportionally
            long displayScore = score.Value.GetDisplayScore(scoringMode.Value);
            int scoreDigits = (int)Math.Ceiling(Math.Log10(displayScore));
            totalScoreText.SetCountWithoutRolling(displayScore);
            totalScoreText.Scale = new Vector2(7f / Math.Max(7, scoreDigits));

            perfectIndicator.Alpha = score.Value.IsPerfect() ? 1 : 0;
            personalBestIndicator.Alpha = isScorePersonalBest(score.Value) ? 1 : 0;
        }

        private bool isScorePersonalBest(IScoreInfo newScore)
        {
            if (newScore.Beatmap == null)
                return false;

            if (api.LocalUser.Value.OnlineID > 0 && !newScore.User.MatchesOnlineID(api.LocalUser.Value))
                return false;

            var personalBest = realm.Run(r =>
                r.GetAllLocalScoresForUser(api.LocalUser.Value.Id)
                 .Filter($@"{nameof(ScoreInfo.BeatmapInfo)}.{nameof(BeatmapInfo.OnlineID)} == $0", newScore.Beatmap.OnlineID)
                 .AsEnumerable()
                 .OrderByDescending(score => score.TotalScore)
                 .ThenBy(score => score.Date)
                 .FirstOrDefault());

            return newScore.MatchesOnlineID(personalBest) || (newScore is ScoreInfo localScore && localScore.Equals(personalBest));
        }

        #region ISerialisableDrawable

        public bool UsesFixedAnchor { get; set; }

        // TODO: temporary to avoid `SkinDeserialisationTest` failures.
        // Remove when the argon skin supplies a default layout of results with this component included.
        public bool IsEditable => false;

        #endregion

        #region IAnimatableSkinnable

        [SettingSource(nameof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.AnimationSequence))]
        public BindableInt GroupNumber { get; } = new BindableInt(2)
        {
            MinValue = 0,
            MaxValue = 10,
        };

        int IAnimatableSkinnable.GroupNumber => GroupNumber.Value;

        public double StartAnimating(double startTime)
        {
            const double transition_duration = 500;

            FinishAnimating();

            scoreContainer.FadeOut()
                          .MoveToOffset(new Vector2(-50, 0));

            double latestTransformEndTime = startTime;

            using (BeginAbsoluteSequence(latestTransformEndTime))
            {
                scoreContainer.FadeIn(transition_duration, Easing.OutQuint)
                              .MoveToOffset(new Vector2(50, 0), transition_duration, Easing.OutQuint);

                totalScoreText.ResetCount();
                totalScoreText.Current.Value = score.Value.GetDisplayScore(scoringMode.Value);
                latestTransformEndTime = totalScoreText.LatestTransformEndTime;
            }

            if (score.Value.MaxCombo == score.Value.GetMaximumAchievableCombo())
            {
                perfectIndicator.FadeOut()
                                .RotateTo(30)
                                .ScaleTo(new Vector2(1.2f));

                using (BeginAbsoluteSequence(latestTransformEndTime))
                {
                    perfectIndicator.FadeIn(150, Easing.OutQuint)
                                    .RotateTo(0, 500, Easing.InOutElastic)
                                    .ScaleTo(Vector2.One, 500, Easing.InOutElastic);

                    latestTransformEndTime = perfectIndicator.LatestTransformEndTime;

                    perfectIndicator.FlashColour(Colour4.White, 1000, Easing.OutSine);
                }
            }
            else
            {
                perfectIndicator.FadeOut()
                                .RotateTo(0)
                                .ScaleTo(Vector2.One);
            }

            if (isScorePersonalBest(score.Value))
            {
                personalBestIndicator.FadeOut()
                                     .ScaleTo(new Vector2(1.2f));

                using (BeginAbsoluteSequence(latestTransformEndTime))
                {
                    personalBestIndicator.FadeIn(150, Easing.OutQuint)
                                         .ScaleTo(Vector2.One, 500, Easing.InOutElastic);
                    personalBestFlash.FadeOutFromOne(1000, Easing.OutSine);

                    latestTransformEndTime = personalBestIndicator.LatestTransformEndTime;
                }
            }
            else
            {
                personalBestIndicator.FadeOut()
                                     .ScaleTo(Vector2.One);
            }

            return latestTransformEndTime;
        }

        public void FinishAnimating()
        {
            scoreContainer.FinishTransforms();
            perfectIndicator.FinishTransforms();
            personalBestIndicator.FinishTransforms();
            personalBestFlash.FinishTransforms();
            totalScoreText.StopRolling();
        }

        #endregion

        public partial class TotalScoreCounter : RollingCounter<long>
        {
            public const double ROLLING_DURATION = 3000;
            public const Easing ROLLING_EASING = Easing.OutPow10;

            protected override double RollingDuration => ROLLING_DURATION;

            protected override Easing RollingEasing => ROLLING_EASING;

            protected override IHasText CreateText()
            {
                return new OsuSpriteText
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Font = OsuFont.TorusAlternate.With(size: 120, weight: FontWeight.Light, fixedWidth: true),
                    Spacing = new Vector2(-5),
                    Colour = TEXT_GRADIENT,
                    Margin = new MarginPadding { Bottom = 15, },
                };
            }

            protected override LocalisableString FormatCount(long count) => count.ToString(@"N0");
        }
    }
}
