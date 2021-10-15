// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Mods;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModGhost : Mod, IUpdatableByPlayfield, IApplicableToScoreProcessor
    {
        public const float CURSOR_ALPHA_TRANSITION_DURATION = 100;
        public override string Name => "Ghost";
        public override string Acronym => "GS";
        public override ModType Type => ModType.Fun;
        public override IconUsage? Icon => FontAwesome.Solid.Ghost;
        public override string Description => "Where's the cursor?";
        public override double ScoreMultiplier => 1;
        private double transitionProgress = 0;
        private float currentCursorAlpha = 1;
        private float startCursorAlpha = 1;
        private float targetCursorAlpha = 0;

        private BindableNumber<int> currentCombo;

        [SettingSource(
            "Hidden at combo",
            "The combo count at which the cursor becomes completely hidden",
            SettingControlType = typeof(SettingsSlider<int, HiddenComboSlider>)
        )]
        public BindableInt HiddenComboCount { get; } = new BindableInt
        {
            Default = 10,
            Value = 10,
            MinValue = 0,
            MaxValue = 50,
        };

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;
        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            if (HiddenComboCount.Value != 0) {
                currentCombo = scoreProcessor.Combo.GetBoundCopy();
                currentCombo.BindValueChanged(combo =>
                {
                    targetCursorAlpha = 1 - (float)combo.NewValue / HiddenComboCount.Value;
                    startCursorAlpha = currentCursorAlpha;
                    transitionProgress = 0;
                }, true);
            }
        }

        public virtual void Update(Playfield playfield)
        {
            if (transitionProgress < CURSOR_ALPHA_TRANSITION_DURATION) {
                transitionProgress += playfield.Time.Elapsed;
                currentCursorAlpha = (float)Interpolation.Lerp(startCursorAlpha, targetCursorAlpha, transitionProgress/CURSOR_ALPHA_TRANSITION_DURATION);
                playfield.Cursor.Alpha = currentCursorAlpha;
            }
        }
    }

    public class HiddenComboSlider : OsuSliderBar<int>
    {
        public override LocalisableString TooltipText => Current.Value == 0 ? "always hidden" : base.TooltipText;
    }
}
