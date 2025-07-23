// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModHardRock : Mod, IApplicableToDifficulty
    {
        public override string Name => "Hard Rock";
        public override string Acronym => "HR";
        public override IconUsage? Icon => OsuIcon.ModHardRock;
        public override ModType Type => ModType.DifficultyIncrease;
        public override LocalisableString Description => "Everything just got a bit harder...";
        public override Type[] IncompatibleMods => new[] { typeof(ModEasy), typeof(ModDifficultyAdjust) };
        public override bool Ranked => UsesDefaultConfiguration;
        public override bool ValidForFreestyleAsRequiredMod => true;

        protected const float ADJUST_RATIO = 1.4f;

        [SettingSource("HP Drain Multiplier", "The multiplier applied to the beatmap's HP drain rate (HP).", SettingControlType = typeof(MultiplierSettingsSlider))]
        public Bindable<double> DrainRateRatio { get; } = new BindableDouble(ADJUST_RATIO)
        {
            // Set a minimum value greater than 1 to ensure Hard Rock always does something
            MinValue = 1.01f,
            MaxValue = 2,
            Precision = 0.01f,
        };

        [SettingSource("Extended Limits", "Adjust difficulty beyond sane limits.")]
        public BindableBool ExtendedLimits { get; } = new BindableBool();

        protected float AdjustLimit => ExtendedLimits.Value ? 11.0f : 10.0f;

        public virtual void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            difficulty.DrainRate = Math.Min(difficulty.DrainRate * (float)DrainRateRatio.Value, AdjustLimit);
        }
    }
}
