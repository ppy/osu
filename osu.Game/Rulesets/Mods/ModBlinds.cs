// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModBlinds<T> : Mod, IApplicableToRulesetContainer<T>, IApplicableToScoreProcessor
            where T : HitObject
    {
        public override string Name => "Blinds";
        public override string ShortenedName => "BL";
        public override FontAwesome Icon => FontAwesome.fa_adjust;
        public override ModType Type => ModType.DifficultyIncrease;
        public override string Description => "Play with blinds on your screen.";
        public override bool Ranked => false;

        public abstract void ApplyToRulesetContainer(RulesetContainer<T> rulesetContainer);
        public abstract void ApplyToScoreProcessor(ScoreProcessor scoreProcessor);
    }
}
