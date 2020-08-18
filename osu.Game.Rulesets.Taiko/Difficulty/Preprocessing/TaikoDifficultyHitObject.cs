// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    public class TaikoDifficultyHitObject : DifficultyHitObject
    {
        public readonly TaikoDifficultyHitObjectRhythm Rhythm;
        public readonly HitType? HitType;

        public bool StaminaCheese;

        public readonly int ObjectIndex;

        public TaikoDifficultyHitObject(HitObject hitObject, HitObject lastObject, HitObject lastLastObject, double clockRate, int objectIndex,
                                        IEnumerable<TaikoDifficultyHitObjectRhythm> commonRhythms)
            : base(hitObject, lastObject, clockRate)
        {
            var currentHit = hitObject as Hit;

            double prevLength = (lastObject.StartTime - lastLastObject.StartTime) / clockRate;

            Rhythm = getClosestRhythm(DeltaTime / prevLength, commonRhythms);
            HitType = currentHit?.Type;

            ObjectIndex = objectIndex;
        }

        private TaikoDifficultyHitObjectRhythm getClosestRhythm(double ratio, IEnumerable<TaikoDifficultyHitObjectRhythm> commonRhythms)
        {
            return commonRhythms.OrderBy(x => Math.Abs(x.Ratio - ratio)).First();
        }
    }
}
