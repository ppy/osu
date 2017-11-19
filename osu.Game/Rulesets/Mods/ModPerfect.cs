// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModPerfect : ModSuddenDeath
    {
        public override string Name => "Perfect";
        public override string ShortenedName => "PF";
        public override string Description => "SS or quit.";
    }

    public abstract class ModPerfect<T> : ModPerfect, IApplicableToRulesetContainer<T>, IApplicableToScoreProcessor
        where T : HitObject
    {
        private ScoreProcessor scoreProcessor;

        public void ApplyToRulesetContainer(RulesetContainer<T> rulesetContainer)
        {
            rulesetContainer.OnJudgement += judgement =>
            {
                if (judgement.Result != HitResult.Perfect)
                {
                    scoreProcessor.ForceFail();
                }
            };
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            this.scoreProcessor = scoreProcessor;
        }
    }
}
