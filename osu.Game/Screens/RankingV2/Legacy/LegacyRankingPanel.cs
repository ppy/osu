// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Skinning;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.RankingV2.Legacy
{
    /// <seealso href="https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L265-L412"/>
    public partial class LegacyRankingPanel : CompositeDrawable, ISerialisableDrawable, IAnimatableSkinnable
    {
        /// <summary>
        /// The baseline position of the <c>ranking-panel</c> sprite in stable code.
        /// See https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L714.
        /// </summary>
        /// <remarks>
        /// In stable, the result screen was more or less just a bag of sprites.
        /// Every single sprite on that screen received absolute screen-space position values.
        /// To make this work in lazer world which has delineated movable components,
        /// many coordinate values taken verbatim from stable must be offset by this amount to make sense.
        /// </remarks>
        private Vector2 baselinePosition;

        private LegacySpriteText scoreText = null!;
        private Container rulesetRankingElements = null!;
        private LegacyRankingElement maxComboElement = null!;
        private LegacyRankingElement accuracyElement = null!;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private readonly Bindable<ScoringMode> scoringMode = new Bindable<ScoringMode>();

        // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L294-L302
        private const float text_x1 = 80;
        private const float text_x2 = 280;

        private const float img_x1 = 40;
        private const float img_x2 = 240;

        private const float row1 = 160;
        private const float row2 = 220;
        private const float row3 = 280;
        private const float row4 = 320;

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, OsuConfigManager config)
        {
            AutoSizeAxes = Axes.Both;

            bool useNewLayout = skin.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value > 1M;
            baselinePosition = new Vector2(0, useNewLayout ? 64 : 46);

            // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L395
            int row4Offset = useNewLayout ? 20 : 8;

            InternalChildren =
            [
                new Sprite
                {
                    Texture = skin.GetTexture(@"ranking-panel"),
                },
                scoreText = new LegacySpriteText(LegacyFont.Score)
                {
                    // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L283-L287
                    Origin = Anchor.Centre,
                    Position = (new Vector2(220, 94) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                    Scale = new Vector2(useNewLayout ? 1.3f : 1.05f),
                    // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L290
                    FixedWidth = true,
                },
                rulesetRankingElements = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L397-L404
                maxComboElement = new LegacyRankingElement
                {
                    ElementName = @"ranking-maxcombo",
                    Origin = Anchor.TopLeft,
                    Position = (new Vector2(img_x1 - 35, row4 - row4Offset) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                    ScoreTextPosition = (new Vector2(text_x1 - 65, row4 + 10) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                    InitialElementScale = Vector2.One,
                    FinalElementScale = Vector2.One,
                },
                // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L406-L412
                accuracyElement = new LegacyRankingElement
                {
                    ElementName = @"ranking-accuracy",
                    Origin = Anchor.TopLeft,
                    Position = (new Vector2(img_x2 - 58, row4 - row4Offset) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                    ScoreTextPosition = (new Vector2(text_x2 - 86, row4 + 10) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                    InitialElementScale = Vector2.One,
                    FinalElementScale = Vector2.One,
                },
            ];

            // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L288-L289
            // value is negated because of https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/Graphics/Sprites/pSpriteText.cs#L209-L218
            // also note that the timing here is significant (this set needs to happen post-load of the text because `Spacing` is set in `LegacySpriteText` BDL)
            if (useNewLayout)
                scoreText.Spacing = new Vector2(2, 0);

            config.BindWith(OsuSetting.ScoreDisplayMode, scoringMode);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            score.BindValueChanged(_ => updateState());
            scoringMode.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            updateTotalScoreText();

            rulesetRankingElements.Clear();

            switch (score.Value.Ruleset.OnlineID)
            {
                case 0:
                    // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L310-L320
                    rulesetRankingElements.AddRange([
                        new LegacyRankingElement
                        {
                            ElementName = @"hit300",
                            ScoreText = $@"{score.Value.GetCount300()}x",
                            Position = (new Vector2(img_x1, row1) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                        new LegacyRankingElement
                        {
                            ElementName = @"hit100",
                            ScoreText = $@"{score.Value.GetCount100()}x",
                            Position = (new Vector2(img_x1, row2) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                        new LegacyRankingElement
                        {
                            ElementName = @"hit50",
                            ScoreText = $@"{score.Value.GetCount50()}x",
                            Position = (new Vector2(img_x1, row3) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                        // Geki count is not available.
                        // Katu count is not available.
                        new LegacyRankingElement
                        {
                            ElementName = @"hit0",
                            ScoreText = $@"{score.Value.GetCountMiss()}x",
                            Position = (new Vector2(img_x2, row3) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                    ]);
                    break;

                case 1:
                    // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L326-L336
                    rulesetRankingElements.AddRange([
                        new LegacyRankingElement
                        {
                            ElementName = @"taiko-hit300",
                            ScoreText = $@"{score.Value.GetCount300()}x",
                            Position = (new Vector2(img_x1, row1) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                        new LegacyRankingElement
                        {
                            ElementName = @"taiko-hit100",
                            ScoreText = $@"{score.Value.GetCount100()}x",
                            Position = (new Vector2(img_x1, row2) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                        new LegacyRankingElement
                        {
                            ElementName = @"taiko-hit0",
                            ScoreText = $@"{score.Value.GetCountMiss()}x",
                            Position = (new Vector2(img_x1, row3) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                        new LegacyRankingElement
                        {
                            ElementName = @"taiko-hit300g",
                            ScoreText = $@"{score.Value.GetCountGeki()}x",
                            Position = (new Vector2(img_x2, row1) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                        // Katu count is not available. lazer merges geki and katu into `LargeBonus` (second hit on strong notes).
                    ]);
                    break;

                case 2:
                    // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L342-L373
                    rulesetRankingElements.AddRange([
                        new LegacyRankingElement
                        {
                            ElementName = @"fruit-orange-overlay",
                            Position = (new Vector2(img_x1, row1) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                            IsOverlay = true,
                        },
                        new LegacyRankingElement
                        {
                            ElementName = @"fruit-orange",
                            ElementColour = Colour4.Orange,
                            ScoreText = $@"{score.Value.GetCount300()}x",
                            Position = (new Vector2(img_x1, row1) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                        new LegacyRankingElement
                        {
                            ElementName = @"fruit-drop",
                            ElementColour = Colour4.YellowGreen,
                            InitialElementScale = new Vector2(1),
                            FinalElementScale = new Vector2(0.6f),
                            ScoreText = $@"{score.Value.GetCount100()}x",
                            Position = (new Vector2(img_x1, row2) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                        new LegacyRankingElement
                        {
                            ElementName = @"fruit-drop",
                            ElementColour = Colour4.LightBlue,
                            InitialElementScale = new Vector2(1),
                            FinalElementScale = new Vector2(0.4f),
                            ElementRotation = 4f,
                            ScoreText = $@"{score.Value.GetCount50()}x",
                            Position = (new Vector2(img_x1, row3) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                        new LegacyRankingElement
                        {
                            ElementName = @"fruit-orange-overlay",
                            ElementRotation = 0.8f,
                            Position = (new Vector2(img_x2, row1) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                            IsOverlay = true,
                        },
                        new LegacyRankingElement
                        {
                            ElementName = @"hit0",
                            Position = (new Vector2(img_x2, row1) - new Vector2(20) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                            InitialElementScale = new Vector2(0.7f),
                            FinalElementScale = new Vector2(0.2f),
                            IsOverlay = true,
                        },
                        new LegacyRankingElement
                        {
                            ElementName = @"fruit-orange",
                            ElementColour = Colour4.LightGray,
                            ElementRotation = 0.8f,
                            ScoreText = $@"{score.Value.GetCountMiss()}x",
                            Position = (new Vector2(img_x2, row1) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                    ]);
                    break;

                case 3:
                    rulesetRankingElements.AddRange([
                        new LegacyRankingElement
                        {
                            ElementName = @"mania-hit300",
                            ScoreText = $@"{score.Value.GetCount300()}x",
                            Position = (new Vector2(img_x1, row1) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                        new LegacyRankingElement
                        {
                            ElementName = @"mania-hit200",
                            ScoreText = $@"{score.Value.GetCountKatu()}x",
                            Position = (new Vector2(img_x1, row2) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                        new LegacyRankingElement
                        {
                            ElementName = @"mania-hit50",
                            ElementColour = Colour4.LightBlue,
                            ScoreText = $@"{score.Value.GetCount50()}x",
                            Position = (new Vector2(img_x1, row3) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                        new LegacyRankingElement
                        {
                            ElementName = @"mania-hit300g",
                            ScoreText = $@"{score.Value.GetCountGeki()}x",
                            Position = (new Vector2(img_x2, row1) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                        new LegacyRankingElement
                        {
                            ElementName = @"mania-hit100",
                            ScoreText = $@"{score.Value.GetCount100()}x",
                            Position = (new Vector2(img_x2, row2) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                        new LegacyRankingElement
                        {
                            ElementName = @"mania-hit0",
                            ScoreText = $@"{score.Value.GetCountMiss()}x",
                            Position = (new Vector2(img_x2, row3) - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        },
                    ]);
                    break;

                default:
                    var ruleset = score.Value.Ruleset.CreateInstance();
                    var basicResults = ruleset.GetValidHitResults().Where(r => r.IsBasic()).Take(6).ToArray();

                    Vector2[] positions =
                    [
                        new Vector2(img_x1, row1),
                        new Vector2(img_x2, row1),
                        new Vector2(img_x1, row2),
                        new Vector2(img_x2, row2),
                        new Vector2(img_x1, row3),
                        new Vector2(img_x2, row3),
                    ];

                    for (int i = 0; i < basicResults.Length; ++i)
                    {
                        var result = basicResults[i];
                        rulesetRankingElements.Add(new LegacyRankingElement
                        {
                            FallbackText = ruleset.GetDisplayNameForHitResult(result).ToUpper(),
                            ElementColour = colours.ForHitResult(result),
                            ScoreText = $@"{score.Value.Statistics.GetValueOrDefault(result)}x",
                            Position = (positions[i] - baselinePosition) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                        });
                    }

                    break;
            }

            // the order of the elements in `rulesetRankingElements` determines the order of playing back the animation in `StartAnimating()`.
            // the overlay elements are supposed to start animating together with their successors in that container.
            // they also need to be *above* their successors draw-order-wise.
            // setting `Depth` does not work here because it also changes the order of iteration over `rulesetRankingElements`.
            // to this end, proxy all overlay elements on top of everything.
            // because proxies are not actual `LegacyRankingElement`s, this has the desired effect.
            // funnily enough stable has a very similar looking hack to fix probably the same issue there too
            // (https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L342).
            // call this "heritage" if you must.
            foreach (var rankingElement in rulesetRankingElements.OfType<LegacyRankingElement>().Where(elem => elem.IsOverlay).ToArray())
                rulesetRankingElements.Add(rankingElement.CreateProxy());

            maxComboElement.ScoreText = FormattableString.Invariant($@"{score.Value.MaxCombo}x");
            accuracyElement.ScoreText = FormattableString.Invariant($@"{score.Value.Accuracy.FloorToDecimalDigits(4) * 100:0.00}%");
        }

        protected override void Update()
        {
            base.Update();

            updateTotalScoreText();
        }

        private double? scoreRevealStartTime;

        private void updateTotalScoreText()
        {
            // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L190-L202
            long totalScore = score.Value.GetDisplayScore(scoringMode.Value);
            string template = new string(Enumerable.Repeat('0', scoringMode.Value == ScoringMode.Standardised ? 7 : 8).ToArray());
            string totalScoreString = totalScore.ToString(template);

            double digitsVisible = (Time.Current - (scoreRevealStartTime ?? double.NegativeInfinity)) / 500;

            if (digitsVisible >= totalScoreString.Length)
            {
                scoreText.Text = totalScoreString;
                return;
            }

            var stringBuilder = new StringBuilder();

            for (int i = 0; i < totalScoreString.Length; ++i)
            {
                if (i >= digitsVisible)
                    // note: this contains a bugfix compared to stable - the `RNG` call there specified bounds of (0, 9) without noticing that the right bound is *exclusive*
                    stringBuilder.Append((char)('0' + RNG.Next(0, 10)));
                else
                    stringBuilder.Append(totalScoreString[i]);
            }

            scoreText.Text = stringBuilder.ToString();
        }

        #region ISerialisableDrawable

        public bool UsesFixedAnchor { get; set; }

        // TODO: temporary to avoid `SkinDeserialisationTest` failures.
        // Remove when the argon skin supplies a default layout of results with this component included.
        public bool IsEditable => false;

        #endregion

        #region IAnimatableSkinnable

        [SettingSource(nameof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.AnimationSequence))]
        public BindableInt GroupNumber { get; } = new BindableInt
        {
            MinValue = 0,
            MaxValue = 10,
        };

        int IAnimatableSkinnable.GroupNumber => GroupNumber.Value;

        public double StartAnimating(double startTime)
        {
            FinishAnimating();

            double latestTransformEndTime = startTime;
            scoreRevealStartTime = latestTransformEndTime;

            foreach (var element in rulesetRankingElements.OfType<LegacyRankingElement>())
                latestTransformEndTime = element.StartAnimating(latestTransformEndTime);

            latestTransformEndTime = maxComboElement.StartAnimating(latestTransformEndTime);
            latestTransformEndTime = accuracyElement.StartAnimating(latestTransformEndTime);

            return latestTransformEndTime;
        }

        public void FinishAnimating()
        {
            foreach (var element in rulesetRankingElements.OfType<LegacyRankingElement>())
                element.FinishAnimating();
            maxComboElement.FinishAnimating();
            accuracyElement.FinishAnimating();
            scoreRevealStartTime = null;
        }

        #endregion

        public partial class LegacyRankingElement : CompositeDrawable
        {
            public const double TEXT_DELAY = 200;
            public const double TRANSITION_DURATION = 300;

            public string? ElementName { get; init; }

            public LocalisableString? FallbackText { get; init; }

            public Colour4 ElementColour { get; init; } = Colour4.White;

            public float ElementRotation { get; init; }

            public Vector2 InitialElementScale { get; init; } = Vector2.One;

            public Vector2 FinalElementScale { get; init; } = new Vector2(0.5f);

            public bool IsOverlay { get; init; }

            private string? scoreText;

            public string? ScoreText
            {
                get => scoreText;
                set
                {
                    scoreText = value;
                    if (IsLoaded)
                        updateState();
                }
            }

            public new Anchor Origin { get; init; } = Anchor.Centre;

            /// <summary>
            /// Used to force the <see cref="ScoreText"/> to appear at a fixed component-space position.
            /// </summary>
            public Vector2? ScoreTextPosition { get; init; }

            private Drawable element = null!;
            private LegacySpriteText scoreTextSprite = null!;

            [BackgroundDependencyLoader]
            private void load(ISkinSource skin)
            {
                AutoSizeAxes = Axes.Both;
                bool useNewLayout = skin.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value > 1M;

                if (!string.IsNullOrEmpty(ElementName))
                {
                    AddInternal(element = new Sprite
                    {
                        // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/RankingElement.cs#L32
                        Texture = skin.GetTextures(ElementName, default, default, true, "-", null, out _).FirstOrDefault(),
                        Anchor = Anchor.TopLeft,
                        Origin = Origin,
                        Scale = FinalElementScale,
                        Colour = ElementColour,
                        Rotation = float.RadiansToDegrees(ElementRotation),
                    });
                }
                else
                {
                    Debug.Assert(FallbackText != null);
                    AddInternal(element = new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.TopLeft,
                        Origin = Origin,
                        Children =
                        [
                            new Circle
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Alpha = 0.3f,
                                Colour = Colour4.Black,
                                Size = new Vector2(5),
                                Scale = new Vector2(2, 1),
                                EdgeEffect = new EdgeEffectParameters
                                {
                                    Type = EdgeEffectType.Shadow,
                                    Radius = 40,
                                    Colour = Colour4.Black,
                                }
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = FallbackText.Value,
                                Font = OsuFont.Style.Subtitle,
                                Colour = ElementColour.Lighten(0.1f),
                            },
                        ],
                    });
                }

                AddInternal(scoreTextSprite = new LegacySpriteText(LegacyFont.Score)
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/RankingElement.cs#L69
                    Scale = new Vector2(1.12f),
                });

                if (ScoreTextPosition != null)
                    scoreTextSprite.Position = ScoreTextPosition.Value - Position;
                else
                {
                    // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/RankingElement.cs#L29
                    // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/RankingElement.cs#L52
                    scoreTextSprite.Position = new Vector2(40, useNewLayout ? -16 : -25) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR;
                }
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                updateState();
            }

            private void updateState()
            {
                scoreTextSprite.Alpha = ScoreText != null ? 1 : 0;
                if (ScoreText != null)
                    scoreTextSprite.Text = ScoreText;
            }

            public double StartAnimating(double startTime)
            {
                FinishAnimating();

                element.ScaleTo(InitialElementScale)
                       .FadeOut();

                scoreTextSprite.MoveToOffset(new Vector2(-40, 0) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR)
                               .FadeOut();

                using (BeginAbsoluteSequence(startTime))
                {
                    // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/RankingElement.cs#L36-L43
                    element.ScaleTo(FinalElementScale, TRANSITION_DURATION, Easing.In)
                           .FadeIn(TRANSITION_DURATION, Easing.In);

                    // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/RankingElement.cs#L70-L79
                    scoreTextSprite.Delay(TEXT_DELAY)
                                   .Then()
                                   .MoveToOffset(new Vector2(40, 0) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR, TRANSITION_DURATION, Easing.Out)
                                   .FadeIn(TRANSITION_DURATION, Easing.Out);

                    return IsOverlay ? startTime : element.LatestTransformEndTime;
                }
            }

            public void FinishAnimating()
            {
                element.FinishTransforms();
                scoreTextSprite.FinishTransforms();
            }
        }
    }
}
