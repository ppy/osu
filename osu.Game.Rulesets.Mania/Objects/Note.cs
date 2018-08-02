// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Judgements;

namespace osu.Game.Rulesets.Mania.Objects
{
    /// <summary>
    /// Represents a hit object which has a single hit press.
    /// </summary>
    public class Note : ManiaHitObject
    {
        public virtual ManiaJudgement Judgement { get; } = new ManiaJudgement();

        protected override IEnumerable<Judgement> CreateJudgements() => new[] { Judgement };
    }
}
