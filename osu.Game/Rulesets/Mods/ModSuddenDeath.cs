// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModSuddenDeath : Mod, IApplicableToScoreProcessor
    {
        public override string Name => "Sudden Death";
        public override string Acronym => "SD";
        public override IconUsage Icon => OsuIcon.ModSuddendeath;
        public override ModType Type => ModType.DifficultyIncrease;
        public override string Description => "Miss and fail.";
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModNoFail), typeof(ModRelax), typeof(ModAutoplay) };

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            scoreProcessor.FailConditions += FailCondition;
        }

        protected virtual bool FailCondition(ScoreProcessor scoreProcessor) => scoreProcessor.Combo.Value == 0;
    }
}
