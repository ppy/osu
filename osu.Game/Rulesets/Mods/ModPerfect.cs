// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModPerfect : ModFailCondition
    {
        public override string Name => "Perfect";
        public override string Acronym => "PF";
        public override IconUsage? Icon => OsuIcon.ModPerfect;
        public override ModType Type => ModType.DifficultyIncrease;
        public override bool Ranked => true;
        public override double ScoreMultiplier => 1;
        public override string Description => "SS or quit.";

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModSuddenDeath)).ToArray();

        protected override bool FailCondition(HealthProcessor healthProcessor, JudgementResult result)
            => result.Type.AffectsAccuracy()
               && result.Type != result.Judgement.MaxResult;
    }
}
