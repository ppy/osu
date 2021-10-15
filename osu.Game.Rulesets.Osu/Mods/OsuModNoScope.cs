// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
    public class OsuModNoScope : Mod, IUpdatableByPlayfield, IApplicableToScoreProcessor
    {
        public const float CURSOR_ALPHA_TRANSITION_DURATION = 100;
        public override string Name => "No Scope";
        public override string Acronym => "NS";
        public override ModType Type => ModType.Fun;
        public override IconUsage? Icon => FontAwesome.Solid.EyeSlash;
        public override string Description => "Where's the cursor?";
        public override double ScoreMultiplier => 1;
        private readonly BindableFloat cursorAlpha = new BindableFloat();
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
            if (HiddenComboCount.Value != 0)
            {
                currentCombo = scoreProcessor.Combo.GetBoundCopy();
                currentCombo.BindValueChanged(combo =>
                {
                    float targetCursorAlpha = (float)Math.Max(1e-3, 1 - (float)combo.NewValue / HiddenComboCount.Value);
                    scoreProcessor.TransformBindableTo(cursorAlpha, targetCursorAlpha, CURSOR_ALPHA_TRANSITION_DURATION, Easing.OutQuint);
                }, true);
            }
        }

        public virtual void Update(Playfield playfield)
        {
            playfield.Cursor.Alpha = cursorAlpha.Value;
        }
    }

    public class HiddenComboSlider : OsuSliderBar<int>
    {
        public override LocalisableString TooltipText => Current.Value == 0 ? "always hidden" : base.TooltipText;
    }
}
