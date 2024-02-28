// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
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
        public const float DEFAULT_SIZE = 0.475f;

        public override Judgement CreateJudgement() => new TaikoJudgement();

        protected override HitWindows CreateHitWindows() => new TaikoHitWindows();
    }
}
