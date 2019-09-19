// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Scoring;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public abstract class TaikoHitObject : HitObject
    {
        /// <summary>
        /// Default size of a drawable taiko hit object.
        /// </summary>
        public const float DEFAULT_SIZE = 0.45f;

        /// <summary>
        /// Scale multiplier for a strong drawable taiko hit object.
        /// </summary>
        public const float STRONG_SCALE = 1.4f;

        /// <summary>
        /// Default size of a strong drawable taiko hit object.
        /// </summary>
        public const float DEFAULT_STRONG_SIZE = DEFAULT_SIZE * STRONG_SCALE;

        /// <summary>
        /// Whether this HitObject is a "strong" type.
        /// Strong hit objects give more points for hitting the hit object with both keys.
        /// </summary>
        public virtual bool IsStrong { get; set; }

        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();

            if (IsStrong)
                AddNested(new StrongHitObject { StartTime = (this as IHasEndTime)?.EndTime ?? StartTime });
        }

        public override Judgement CreateJudgement() => new TaikoJudgement();

        protected override HitWindows CreateHitWindows() => new TaikoHitWindows();
    }
}
