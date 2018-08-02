// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Taiko.Judgements;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class Swell : TaikoHitObject, IHasEndTime
    {
        public double EndTime => StartTime + Duration;

        public double Duration { get; set; }

        /// <summary>
        /// The number of hits required to complete the swell successfully.
        /// </summary>
        public int RequiredHits = 10;

        public TaikoJudgement Judgement { get; private set; }

        private readonly List<TaikoIntermediateSwellJudgement> intermediateJudgements = new List<TaikoIntermediateSwellJudgement>();
        public IReadOnlyList<TaikoIntermediateSwellJudgement> IntermediateJudgements => intermediateJudgements;

        protected override IEnumerable<Judgement> CreateJudgements()
        {
            yield return Judgement = new TaikoJudgement();

            for (int i = 0; i < RequiredHits; i++)
            {
                var intermediate = new TaikoIntermediateSwellJudgement();
                intermediateJudgements.Add(intermediate);

                yield return intermediate;
            }
        }
    }
}
