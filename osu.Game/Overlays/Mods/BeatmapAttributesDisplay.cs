// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    /// <summary>
    /// On the mod select overlay, this provides a local updating view of BPM, star rating and other
    /// difficulty attributes so the user can have a better insight into what mods are changing.
    /// </summary>
    public partial class BeatmapAttributesDisplay : ModFooterInformationDisplay, IHasCustomTooltip
    {
        private StarRatingDisplay starRatingDisplay = null!;
        private BPMDisplay bpmDisplay = null!;

        private VerticalAttributeDisplay circleSizeDisplay = null!;
        private VerticalAttributeDisplay drainRateDisplay = null!;
        private VerticalAttributeDisplay approachRateDisplay = null!;
        private VerticalAttributeDisplay overallDifficultyDisplay = null!;

        public Bindable<IBeatmapInfo?> BeatmapInfo { get; } = new Bindable<IBeatmapInfo?>();

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        public BindableBool Collapsed { get; } = new BindableBool(true);

        private ModSettingChangeTracker? modSettingChangeTracker;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        private IBindable<RulesetInfo> gameRuleset = null!;

        private CancellationTokenSource? cancellationSource;
        private IBindable<StarDifficulty?> starDifficulty = null!;

        private AdjustedAttributesTooltip rateAdjustTooltip = null!;

        public ITooltip GetCustomTooltip() => rateAdjustTooltip;
        public object TooltipContent => this;


        private const float transition_duration = 250;

        [BackgroundDependencyLoader]
        private void load()
        {
            const float shear = ShearedOverlayContainer.SHEAR;

            rateAdjustTooltip = new AdjustedAttributesTooltip();

            LeftContent.AddRange(new Drawable[]
            {
                starRatingDisplay = new StarRatingDisplay(default, animated: true)
                {
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                    Shear = new Vector2(-shear, 0),
                },
                bpmDisplay = new BPMDisplay
                {
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                    Shear = new Vector2(-shear, 0),
                    AutoSizeAxes = Axes.Y,
                    Width = 75,
                }
            });

            RightContent.Alpha = 0;
            RightContent.AddRange(new Drawable[]
            {
                circleSizeDisplay = new VerticalAttributeDisplay("CS") { Shear = new Vector2(-shear, 0), },
                drainRateDisplay = new VerticalAttributeDisplay("HP") { Shear = new Vector2(-shear, 0), },
                approachRateDisplay = new VerticalAttributeDisplay("AR") { Shear = new Vector2(-shear, 0), },
                overallDifficultyDisplay = new VerticalAttributeDisplay("OD") { Shear = new Vector2(-shear, 0), },
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            mods.BindValueChanged(_ =>
            {
                modSettingChangeTracker?.Dispose();
                modSettingChangeTracker = new ModSettingChangeTracker(mods.Value);
                modSettingChangeTracker.SettingChanged += _ => updateValues();
                updateValues();
            }, true);

            BeatmapInfo.BindValueChanged(_ => updateValues(), true);

            Collapsed.BindValueChanged(_ =>
            {
                // Only start autosize animations on first collapse toggle. This avoids an ugly initial presentation.
                startAnimating();
                updateCollapsedState();
            });

            gameRuleset = game.Ruleset.GetBoundCopy();
            gameRuleset.BindValueChanged(_ => updateValues());

            BeatmapInfo.BindValueChanged(_ => updateValues(), true);

            rateAdjustTooltip.AddAttribute("AR");
            rateAdjustTooltip.AddAttribute("OD");

            updateCollapsedState();
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
            Content.AutoSizeEasing = Easing.OutQuint;
            Content.AutoSizeDuration = transition_duration;
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

            BeatmapDifficulty originalDifficulty = new BeatmapDifficulty(BeatmapInfo.Value.Difficulty);

            foreach (var mod in mods.Value.OfType<IApplicableToDifficulty>())
                mod.ApplyToDifficulty(originalDifficulty);

            Ruleset ruleset = gameRuleset.Value.CreateInstance();
            BeatmapDifficulty adjustedDifficulty = ruleset.GetRateAdjustedDisplayDifficulty(originalDifficulty, rate);

            rateAdjustTooltip.UpdateAttribute("AR", originalDifficulty.ApproachRate, adjustedDifficulty.ApproachRate);
            rateAdjustTooltip.UpdateAttribute("OD", originalDifficulty.OverallDifficulty, adjustedDifficulty.OverallDifficulty);

            approachRateDisplay.AdjustType.Value = VerticalAttributeDisplay.CalculateEffect(originalDifficulty.ApproachRate, adjustedDifficulty.ApproachRate);
            overallDifficultyDisplay.AdjustType.Value = VerticalAttributeDisplay.CalculateEffect(originalDifficulty.OverallDifficulty, adjustedDifficulty.OverallDifficulty);

            circleSizeDisplay.Current.Value = adjustedDifficulty.CircleSize;
            drainRateDisplay.Current.Value = adjustedDifficulty.DrainRate;
            approachRateDisplay.Current.Value = adjustedDifficulty.ApproachRate;
            overallDifficultyDisplay.Current.Value = adjustedDifficulty.OverallDifficulty;
        });

        private void updateCollapsedState()
        {
            RightContent.FadeTo(Collapsed.Value && !IsHovered ? 0 : 1, transition_duration, Easing.OutQuint);
        }

        private partial class BPMDisplay : RollingCounter<double>
        {
            protected override double RollingDuration => 500;

            protected override LocalisableString FormatCount(double count) => count.ToLocalisableString("0 BPM");

            protected override OsuSpriteText CreateSpriteText() => new OsuSpriteText
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                Font = OsuFont.Default.With(size: 20, weight: FontWeight.SemiBold),
                UseFullGlyphHeight = false,
            };
        }
    }
}
