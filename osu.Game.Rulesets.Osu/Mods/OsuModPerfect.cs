// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModPerfect : ModPerfect
    {
        [SettingSource("Require spinner MAX bonus", "Fail if you don't achieve MAX bonus on spinners.")]
        public BindableBool RequireSpinnerMax { get; } = new BindableBool();

        public override bool Ranked => base.Ranked && RequireSpinnerMax.IsDefault;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(OsuModSpunOut)).ToArray();

        protected override bool FailCondition(HealthProcessor healthProcessor, JudgementResult result)
        {
            // Check base fail condition first
            if (base.FailCondition(healthProcessor, result))
                return true;

            // If spinner MAX bonus is required, check if any bonus tick was missed
            if (RequireSpinnerMax.Value && result.HitObject is SpinnerBonusTick)
            {
                // Fail if any bonus tick was not hit
                if (!result.IsHit)
                    return true;
            }

            return false;
        }
    }
}
