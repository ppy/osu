// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Catch.Difficulty.Preprocessing
{
    public class CatchDifficultyHitObject : DifficultyHitObject
    {
        private const float normalized_hitobject_radius = 41.0f;

        public new CatchHitObject BaseObject => (CatchHitObject)base.BaseObject;

        public new CatchHitObject LastObject => (CatchHitObject)base.LastObject;

        public readonly float NormalizedPosition;
        public readonly float LastNormalizedPosition;

        /// <summary>
        /// Milliseconds elapsed since the start time of the previous <see cref="CatchDifficultyHitObject"/>, with a minimum of 25ms.
        /// </summary>
        public readonly double StrainTime;

        public CatchDifficultyHitObject(HitObject hitObject, HitObject lastObject, double clockRate, float halfCatcherWidth)
            : base(hitObject, lastObject, clockRate)
        {
            // We will scale everything by this factor, so we can assume a uniform CircleSize among beatmaps.
            var scalingFactor = normalized_hitobject_radius / halfCatcherWidth;

            NormalizedPosition = BaseObject.X * CatchPlayfield.BASE_WIDTH * scalingFactor;
            LastNormalizedPosition = LastObject.X * CatchPlayfield.BASE_WIDTH * scalingFactor;

            // Every strain interval is hard capped at the equivalent of 600 BPM streaming speed as a safety measure
            StrainTime = Math.Max(25, DeltaTime);
        }
    }
}
