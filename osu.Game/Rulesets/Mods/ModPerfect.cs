// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModPerfect : ModSuddenDeath, IApplicableToScoreProcessor
    {
        public override string Name => "Perfect";
        public override string ShortenedName => "PF";
        public override string Description => "SS or quit.";

        public bool onFailCheck(ScoreProcessor scoreProcessor)
        {
            return scoreProcessor.Accuracy.Value != 1;
        }

        public virtual void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            scoreProcessor.FailChecker += onFailCheck;
        }
    }
}
