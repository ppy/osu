// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModPerfect : ModSuddenDeath, IApplicableRestartOnFail
    {
        public override string Name => "Perfect";
        public override string ShortenedName => "PF";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_perfect;
        public override string Description => "SS or quit.";

        public bool AllowRestart => true;

        protected override bool FailCondition(ScoreProcessor scoreProcessor) => scoreProcessor.Accuracy.Value != 1;
    }
}
