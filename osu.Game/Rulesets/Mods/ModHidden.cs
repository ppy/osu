// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModHidden : ModWithVisibilityAdjustment, IApplicableToScoreProcessor
    {
        public override string Name => "Hidden";
        public override string Acronym => "HD";
        public override IconUsage? Icon => OsuIcon.ModHidden;
        public override ModType Type => ModType.DifficultyIncrease;

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            // Default value of ScoreProcessor's Rank in Hidden Mod should be SS+
            scoreProcessor.Rank.Value = Grade.XH;
        }

        public Grade AdjustGrade(Grade grade, double accuracy)
        {
            switch (grade)
            {
                case Grade.X:
                    return Grade.XH;

                case Grade.S:
                    return Grade.SH;

                default:
                    return grade;
            }
        }
    }
}
