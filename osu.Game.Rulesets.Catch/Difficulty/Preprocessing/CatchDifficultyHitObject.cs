// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Catch.Difficulty.Preprocessing
{
    public class CatchDifficultyHitObject : DifficultyHitObject
    {
        private const float normalized_hitobject_radius = 41.0f;

        public new PalpableCatchHitObject BaseObject => (PalpableCatchHitObject)base.BaseObject;

        public new PalpableCatchHitObject LastObject => (PalpableCatchHitObject)base.LastObject;

        public readonly float NormalizedPosition;
        public readonly float LastNormalizedPosition;

        /// <summary>
        /// Milliseconds elapsed since the start time of the previous <see cref="CatchDifficultyHitObject"/>, with a minimum of 40ms.
        /// </summary>
        public readonly double StrainTime;

        public CatchDifficultyHitObject(HitObject hitObject, HitObject lastObject, double clockRate, float halfCatcherWidth, List<DifficultyHitObject> objects, int index)
            : base(hitObject, lastObject, clockRate, objects, index)
        {
            // We will scale everything by this factor, so we can assume a uniform CircleSize among beatmaps.
            float scalingFactor = normalized_hitobject_radius / halfCatcherWidth;

            NormalizedPosition = BaseObject.EffectiveX * scalingFactor;
            LastNormalizedPosition = LastObject.EffectiveX * scalingFactor;

            // Every strain interval is hard capped at the equivalent of 375 BPM streaming speed as a safety measure
            StrainTime = Math.Max(40, DeltaTime);
        }
    }
}
