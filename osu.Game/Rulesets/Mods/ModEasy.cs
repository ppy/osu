// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModEasy : Mod, IApplicableToDifficulty, IApplicableToScoreProcessor
    {
        private int Lives;
        public override string Name => "Easy";
        public override string Acronym => "EZ";
        public override IconUsage Icon => OsuIcon.ModEasy;
        public override ModType Type => ModType.DifficultyReduction;
        public override double ScoreMultiplier => 0.5;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModHardRock) };
              
        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            const float ratio = 0.5f;
            difficulty.CircleSize *= ratio;
            difficulty.ApproachRate *= ratio;
            difficulty.DrainRate *= ratio;
            difficulty.OverallDifficulty *= ratio;
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            //Note : The lives has to be instaciated here in order to prevent the values from different plays to interfear
            //with each other / not reseting after a restart , as this method is called once a play starts (to my knowlegde).
            //This will be better implemented with a List<double> once I know how to reliably get the game time and update it.
            //If you know any information about that, please contact me because I didn't find a sollution to that.
            Lives = 2;
            scoreProcessor.Health.ValueChanged += valueChanged =>
            {
                if (scoreProcessor.Health.Value == scoreProcessor.Health.MinValue && Lives > 0)
                {
                    Lives--;
                    scoreProcessor.Health.Value = scoreProcessor.Health.MaxValue;
                }
            };
        }
    }
}
