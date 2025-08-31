// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.ComponentModel;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Localisation;
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

        public override IEnumerable<(LocalisableString setting, LocalisableString value)> SettingDescription
        {
            get
            {
                foreach (var setting in base.SettingDescription)
                    yield return setting;

                if (!HitWindowAdjustment.IsDefault)
                    yield return ("Hit Window Adjustment", HitWindowAdjustment.GetDescription());
            }
        }

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
            [Description("Off")]
            Off,

            [Description("Hard Rock")]
            HardRock,

            [Description("Easy")]
            Easy
        }
    }
}
