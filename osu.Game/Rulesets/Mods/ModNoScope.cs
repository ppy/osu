// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModNoScope : Mod, IApplicableToScoreProcessor, IApplicableToPlayer, IApplicableToHUD
    {
        public override string Name => "No Scope";
        public override string Acronym => "NS";
        public override ModType Type => ModType.Fun;
        public override IconUsage? Icon => FontAwesome.Solid.EyeSlash;
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;

        /// <summary>
        /// Slightly higher than the cutoff for <see cref="Drawable.IsPresent"/>.
        /// </summary>
        protected const float MIN_ALPHA = 0.0002f;

        protected const float TRANSITION_DURATION = 100;

        protected readonly BindableNumber<int> CurrentCombo = new BindableInt();

        protected readonly IBindable<bool> IsBreakTime = new Bindable<bool>();

        protected readonly BindableFloat MinimumSpectateAlpha = new BindableFloat();

        private float lastUsedMinimumSpectateAlpha;

        private float comboBasedPercentage;

        [SettingSource(
            "Hidden at combo",
            "The combo count at which the cursor becomes completely hidden",
            SettingControlType = typeof(SettingsSlider<int, HiddenComboSlider>)
        )]
        public abstract BindableInt HiddenComboCount { get; }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;

        public void ApplyToPlayer(Player player)
        {
            IsBreakTime.BindTo(player.IsBreakTime);
        }

        private void updatePercentage()
        {
            float t;
            if (HiddenComboCount.Value == 0)
                t = 1;
            else
                t = Math.Clamp((float)CurrentCombo.Value / HiddenComboCount.Value, 0.0f, 1.0f);

            comboBasedPercentage = 1.0f - t;
        }

        protected float ComputeNewAlpha(float oldAlpha, bool mustBeVisible, double deltaTime)
        {
            float oldPercentage = (oldAlpha - lastUsedMinimumSpectateAlpha) / (1 - lastUsedMinimumSpectateAlpha);
            float targetPercentage = mustBeVisible ? 1 : comboBasedPercentage;
            float newPercentage = (float)Interpolation.Lerp(oldPercentage, targetPercentage, Math.Clamp(deltaTime / TRANSITION_DURATION, 0, 1));

            float minAlpha = Math.Clamp(MinimumSpectateAlpha.Value, MIN_ALPHA, 0.999f);
            float newAlpha = newPercentage * (1 - minAlpha) + minAlpha;
            lastUsedMinimumSpectateAlpha = minAlpha;

            return newAlpha;
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            if (HiddenComboCount.Value == 0) return;

            CurrentCombo.BindTo(scoreProcessor.Combo);
            CurrentCombo.BindValueChanged(_ => updatePercentage(), true);
        }

        public void ApplyToHUD(HUDOverlay overlay)
        {
            if (overlay.ReplayLoaded.Value)
            {
                overlay.PlayerSettingsOverlay.VisualSettings.ShowNoScopeSettings.Value = true;
                MinimumSpectateAlpha.BindTo(overlay.PlayerSettingsOverlay.VisualSettings.MinimumNoScopeAlpha);
                MinimumSpectateAlpha.BindValueChanged(_ => updatePercentage(), true);
            }
        }
    }

    public partial class HiddenComboSlider : RoundedSliderBar<int>
    {
        public override LocalisableString TooltipText => Current.Value == 0 ? "always hidden" : base.TooltipText;
    }
}
