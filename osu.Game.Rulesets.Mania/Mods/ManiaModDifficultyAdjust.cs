// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModDifficultyAdjust : ModDifficultyAdjust, IApplicableToHitObject
    {
        [SettingSource("Hit Window Adjustment", "Adjust hit window timings as if Hard Rock or Easy were enabled.")]
        public Bindable<HitWindowAdjustmentType> HitWindowAdjustment { get; } = new Bindable<HitWindowAdjustmentType>(HitWindowAdjustmentType.Off);

        public static double CurrentHitWindowDifficultyMultiplier = 1;

        void IApplicableToHitObject.ApplyToHitObject(HitObject hitObject)
        {
            switch (HitWindowAdjustment.Value)
            {
                case HitWindowAdjustmentType.HardRock:
                    CurrentHitWindowDifficultyMultiplier = 1.4;
                    break;

                case HitWindowAdjustmentType.Easy:
                    CurrentHitWindowDifficultyMultiplier = 1 / 1.4;
                    break;

                case HitWindowAdjustmentType.Off:
                default:
                    CurrentHitWindowDifficultyMultiplier = 1;
                    break;
            }

            switch (hitObject)
            {
                case Note:
                    ((ManiaHitWindows)hitObject.HitWindows).DifficultyMultiplier = CurrentHitWindowDifficultyMultiplier;
                    break;

                case HoldNote hold:
                    ((ManiaHitWindows)hold.Head.HitWindows).DifficultyMultiplier = CurrentHitWindowDifficultyMultiplier;
                    ((ManiaHitWindows)hold.Tail.HitWindows).DifficultyMultiplier = CurrentHitWindowDifficultyMultiplier;
                    break;
            }
        }

        public enum HitWindowAdjustmentType
        {
            Off,
            HardRock,
            Easy
        }
    }
}
