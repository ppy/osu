// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Graphics;
using System.Threading;
using osu.Framework.Input.Events;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Mods
{
    /// <summary>
    /// On the mod select overlay, this provides a local updating view of BPM, star rating and other
    /// difficulty attributes so the user can have a better insight into what mods are changing.
    /// </summary>
    public partial class BeatmapAttributesDisplay : CompositeDrawable
    {
        private Container content = null!;
        private Container innerContent = null!;

        private Box background = null!;
        private Box innerBackground = null!;

        private StarRatingDisplay starRatingDisplay = null!;
        private BPMDisplay bpmDisplay = null!;

        private FillFlowContainer<VerticalAttributeDisplay> outerContent = null!;
        private VerticalAttributeDisplay circleSizeDisplay = null!;
        private VerticalAttributeDisplay drainRateDisplay = null!;
        private VerticalAttributeDisplay approachRateDisplay = null!;
        private VerticalAttributeDisplay overallDifficultyDisplay = null!;

        private const float transition_duration = 250;

        public Bindable<IBeatmapInfo?> BeatmapInfo { get; } = new Bindable<IBeatmapInfo?>();

        public BindableBool Collapsed { get; } = new BindableBool(true);

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        private ModSettingChangeTracker? modSettingChangeTracker;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        [Resolved]
        private OsuGameBase game { get; set; } = null!;
        private IBindable<RulesetInfo> gameRuleset = null!;

        private CancellationTokenSource? cancellationSource;
        private IBindable<StarDifficulty?> starDifficulty = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            const float shear = ShearedOverlayContainer.SHEAR;

            AutoSizeAxes = Axes.Both;
            InternalChild = content = new Container
            {
                Origin = Anchor.BottomRight,
                Anchor = Anchor.BottomRight,
                AutoSizeAxes = Axes.X,
                Height = ShearedButton.HEIGHT,
                Shear = new Vector2(shear, 0),
                CornerRadius = ShearedButton.CORNER_RADIUS,
                BorderThickness = ShearedButton.BORDER_THICKNESS,
                Masking = true,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    new FillFlowContainer // divide inner and outer content
                    {
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.BottomLeft,
                        AutoSizeAxes = Axes.X,
                        RelativeSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Children = new Drawable[]
                        {
                            innerContent = new Container
                            {
                                AutoSizeAxes = Axes.X,
                                RelativeSizeAxes = Axes.Y,
                                BorderThickness = ShearedButton.BORDER_THICKNESS,
                                CornerRadius = ShearedButton.CORNER_RADIUS,
                                Masking = true,
                                Children = new Drawable[]
                                {
                                    innerBackground = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both
                                    },
                                    new Container // actual inner content
                                    {
                                        Origin = Anchor.Centre,
                                        Anchor = Anchor.Centre,
                                        Width = 140,
                                        RelativeSizeAxes = Axes.Y,
                                        Margin = new MarginPadding { Horizontal = 15 },
                                        Children = new Drawable[]
                                        {
                                            starRatingDisplay = new StarRatingDisplay(default, animated: true)
                                            {
                                                Origin = Anchor.CentreLeft,
                                                Anchor = Anchor.CentreLeft,
                                                Shear = new Vector2(-shear, 0),
                                            },
                                            bpmDisplay = new BPMDisplay
                                            {
                                                Origin = Anchor.CentreRight,
                                                Anchor = Anchor.CentreRight,
                                                Shear = new Vector2(-shear, 0),
                                            }
                                        }
                                    }
                                }
                            },
                            outerContent = new FillFlowContainer<VerticalAttributeDisplay>
                            {
                                Alpha = 0,
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.X,
                                RelativeSizeAxes = Axes.Y,
                                Direction = FillDirection.Horizontal,
                                Children = new[]
                                {
                                    circleSizeDisplay = new VerticalAttributeDisplay("CS")
                                    {
                                        Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0),
                                    },
                                    drainRateDisplay = new VerticalAttributeDisplay("HP")
                                    {
                                        Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0),
                                    },
                                    approachRateDisplay = new VerticalAttributeDisplay("AR")
                                    {
                                        Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0),
                                    },
                                    overallDifficultyDisplay = new VerticalAttributeDisplay("OD")
                                    {
                                        Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0),
                                    },
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            background.Colour = colourProvider.Background4;
            innerBackground.Colour = colourProvider.Background3;
            Color4 glowColour = colourProvider.Background1;

            content.BorderColour = ColourInfo.GradientVertical(background.Colour, glowColour);
            innerContent.BorderColour = ColourInfo.GradientVertical(innerBackground.Colour, glowColour);

            mods.BindValueChanged(_ =>
            {
                modSettingChangeTracker?.Dispose();

                modSettingChangeTracker = new ModSettingChangeTracker(mods.Value);
                modSettingChangeTracker.SettingChanged += _ => updateValues();
                updateValues();
            }, true);

            Collapsed.BindValueChanged(_ =>
            {
                // Only start autosize animations on first collapse toggle. This avoids an ugly initial presentation.
                startAnimating();

                updateCollapsedState();
            });

            gameRuleset = game.Ruleset.GetBoundCopy();
            gameRuleset.BindValueChanged(_ => updateValues());

            BeatmapInfo.BindValueChanged(_ => updateValues(), true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            startAnimating();
            updateCollapsedState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateCollapsedState();
            base.OnHoverLost(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e) => true;

        protected override bool OnClick(ClickEvent e) => true;

        private void startAnimating()
        {
            content.AutoSizeEasing = Easing.OutQuint;
            content.AutoSizeDuration = transition_duration;
        }

        private void updateValues() => Scheduler.AddOnce(() =>
        {
            if (BeatmapInfo.Value == null)
                return;

            cancellationSource?.Cancel();

            starDifficulty = difficultyCache.GetBindableDifficulty(BeatmapInfo.Value, (cancellationSource = new CancellationTokenSource()).Token);
            starDifficulty.BindValueChanged(s =>
            {
                starRatingDisplay.Current.Value = s.NewValue ?? default;

                if (!starRatingDisplay.IsPresent)
                    starRatingDisplay.FinishTransforms(true);
            });

            double rate = 1;
            foreach (var mod in mods.Value.OfType<IApplicableToRate>())
                rate = mod.ApplyToRate(0, rate);

            bpmDisplay.Current.Value = BeatmapInfo.Value.BPM * rate;

            var moddedDifficulty = new BeatmapDifficulty(BeatmapInfo.Value.Difficulty);

            foreach (var mod in mods.Value.OfType<IApplicableToDifficulty>())
                mod.ApplyToDifficulty(moddedDifficulty);

            Ruleset ruleset = gameRuleset.Value.CreateInstance();
            var rateAdjustedDifficulty = ruleset.GetRateAdjustedDifficulty(moddedDifficulty, rate);

            approachRateDisplay.AdjustType.Value = VerticalAttributeDisplay.CalculateEffect(moddedDifficulty.ApproachRate, rateAdjustedDifficulty.ApproachRate);
            overallDifficultyDisplay.AdjustType.Value = VerticalAttributeDisplay.CalculateEffect(moddedDifficulty.OverallDifficulty, rateAdjustedDifficulty.OverallDifficulty);

            circleSizeDisplay.Current.Value = rateAdjustedDifficulty.CircleSize;
            drainRateDisplay.Current.Value = rateAdjustedDifficulty.DrainRate;
            approachRateDisplay.Current.Value = rateAdjustedDifficulty.ApproachRate;
            overallDifficultyDisplay.Current.Value = rateAdjustedDifficulty.OverallDifficulty;
        });

        private void updateCollapsedState()
        {
            outerContent.FadeTo(Collapsed.Value && !IsHovered ? 0 : 1, transition_duration, Easing.OutQuint);
        }

        private partial class BPMDisplay : RollingCounter<double>
        {
            protected override double RollingDuration => 500;

            protected override LocalisableString FormatCount(double count) => count.ToLocalisableString("0 BPM");

            protected override OsuSpriteText CreateSpriteText() => new OsuSpriteText
            {
                Font = OsuFont.Default.With(size: 20, weight: FontWeight.SemiBold),
                UseFullGlyphHeight = false,
            };
        }
    }
}
