// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Mods;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModGhost : Mod, IUpdatableByPlayfield, IApplicableToScoreProcessor
    {
        public override string Name => "Ghost";
        public override string Acronym => "G";
        public override ModType Type => ModType.Fun;
        public override IconUsage? Icon => FontAwesome.Solid.Ghost;
        public override string Description => "Where's the cursor?";
        public override double ScoreMultiplier => 1;
        
        private readonly BindableNumber<float> ghostAlpha = new BindableFloat(0);
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
            currentCombo = scoreProcessor.Combo.GetBoundCopy();
            currentCombo.BindValueChanged(combo =>
            {
                float dimFactor = HiddenComboCount.Value == 0 ? 1 : (float)combo.NewValue / HiddenComboCount.Value;

                scoreProcessor.TransformBindableTo(ghostAlpha, 1-dimFactor, 100, Easing.OutQuint);
            }, true);
        }


        public virtual void Update(Playfield playfield)
        {
            playfield.Cursor.Alpha = ghostAlpha.Value;
        }
        
    }

    public class HiddenComboSlider : OsuSliderBar<int>
    {
        public override LocalisableString TooltipText => Current.Value == 0 ? "always hidden" : base.TooltipText;
    }
}
