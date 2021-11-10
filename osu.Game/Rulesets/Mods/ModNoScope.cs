// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModNoScope : Mod, IApplicableToScoreProcessor, IApplicableToPlayer
    {
        public override string Name => "No Scope";
        public override string Acronym => "NS";
        public override ModType Type => ModType.Fun;
        public override IconUsage? Icon => FontAwesome.Solid.EyeSlash;
        public override double ScoreMultiplier => 1;

        /// <summary>
        /// Slightly higher than the cutoff for <see cref="Drawable.IsPresent"/>.
        /// </summary>
        protected const float MIN_ALPHA = 0.0002f;

        protected const float TRANSITION_DURATION = 100;

        protected BindableNumber<int> CurrentCombo;

        protected IBindable<bool> IsBreakTime;

        protected float ComboBasedAlpha;

        public abstract BindableInt HiddenComboCount { get; }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;

        public void ApplyToPlayer(Player player)
        {
            IsBreakTime = player.IsBreakTime.GetBoundCopy();
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            if (HiddenComboCount.Value == 0) return;

            CurrentCombo = scoreProcessor.Combo.GetBoundCopy();
            CurrentCombo.BindValueChanged(combo =>
            {
                ComboBasedAlpha = Math.Max(MIN_ALPHA, 1 - (float)combo.NewValue / HiddenComboCount.Value);
            }, true);
        }
    }

    public class HiddenComboSlider : OsuSliderBar<int>
    {
        public override LocalisableString TooltipText => Current.Value == 0 ? "always hidden" : base.TooltipText;
    }
}
