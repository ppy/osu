﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class DrumRollTick : TaikoStrongableHitObject
    {
        public readonly DrumRoll Parent;

        /// <summary>
        /// Whether this is the first (initial) tick of the slider.
        /// </summary>
        public bool FirstTick;

        /// <summary>
        /// The length (in milliseconds) between this tick and the next.
        /// <para>Half of this value is the hit window of the tick.</para>
        /// </summary>
        public double TickSpacing;

        /// <summary>
        /// The time allowed to hit this tick.
        /// </summary>
        public double HitWindow => TickSpacing / 2;

        public DrumRollTick(DrumRoll parent)
        {
            Parent = parent;
        }

        public override Judgement CreateJudgement() => new TaikoDrumRollTickJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        public override double MaximumJudgementOffset => HitWindow;

        protected override StrongNestedHitObject CreateStrongNestedHit(double startTime) => new StrongNestedHit(this)
        {
            StartTime = startTime,
            Samples = Samples
        };

        public class StrongNestedHit : StrongNestedHitObject
        {
            public StrongNestedHit(TaikoHitObject parent)
                : base(parent)
            {
            }
        }
    }
}
