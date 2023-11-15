// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Pippidon.Objects
{
    public class PippidonHitObject : HitObject
    {
        /// <summary>
        /// Range = [-1,1]
        /// </summary>
        public int Lane;

        public override Judgement CreateJudgement() => new Judgement();

        protected override void CopyFrom(HitObject other, IDictionary<object, object> referenceLookup = null)
        {
            base.CopyFrom(other, referenceLookup);

            if (other is not PippidonHitObject pippidonHitObject)
                throw new ArgumentException($"{nameof(other)} must be of type {nameof(PippidonHitObject)}");

            Lane = pippidonHitObject.Lane;
        }

        protected override HitObject CreateInstance() => new PippidonHitObject();
    }
}
