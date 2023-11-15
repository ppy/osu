// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mania.Objects
{
    /// <summary>
    /// Represents a hit object which has a single hit press.
    /// </summary>
    public class Note : ManiaHitObject
    {
        public override Judgement CreateJudgement() => new ManiaJudgement();

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            AddNested(new NotePerfectBonus { StartTime = StartTime });
        }

        protected override HitObject CreateInstance() => new Note();
    }
}
