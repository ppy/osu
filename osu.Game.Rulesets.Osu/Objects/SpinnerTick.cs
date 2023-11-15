// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class SpinnerTick : OsuHitObject
    {
        /// <summary>
        /// Duration of the <see cref="Spinner"/> containing this spinner tick.
        /// </summary>
        public double SpinnerDuration { get; set; }

        public override Judgement CreateJudgement() => new OsuSpinnerTickJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        public override double MaximumJudgementOffset => SpinnerDuration;

        protected override void CopyFrom(HitObject other, IDictionary<object, object>? referenceLookup = null)
        {
            base.CopyFrom(other, referenceLookup);

            if (other is not SpinnerTick spinnerTick)
                throw new ArgumentException($"{nameof(other)} must be of type {nameof(SpinnerTick)}");

            SpinnerDuration = spinnerTick.SpinnerDuration;
        }

        protected override HitObject CreateInstance() => new SpinnerTick();

        public class OsuSpinnerTickJudgement : OsuJudgement
        {
            public override HitResult MaxResult => HitResult.SmallBonus;
        }
    }
}
