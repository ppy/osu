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
    public class OsuModSuddenDeath : ModSuddenDeath
    {
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[]
        {
            typeof(OsuModTargetPractice),
        }).ToArray();

        [SettingSource("Fail when missing on a slider tail")]
        public BindableBool SliderTailMiss { get; } = new BindableBool();

        protected override bool FailCondition(HealthProcessor healthProcessor, JudgementResult result)
        {
            if (SliderTailMiss.Value && result.HitObject is SliderTailCircle && result.Type == HitResult.IgnoreMiss)
                return true;

            return result.Type.AffectsCombo() && !result.IsHit;
        }
    }
}
