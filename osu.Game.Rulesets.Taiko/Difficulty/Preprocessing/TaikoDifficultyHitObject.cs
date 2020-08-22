// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
        public readonly int ObjectIndex;

        public bool StaminaCheese;

        public TaikoDifficultyHitObject(HitObject hitObject, HitObject lastObject, HitObject lastLastObject, double clockRate, int objectIndex)
            : base(hitObject, lastObject, clockRate)
        {
            var currentHit = hitObject as Hit;

            double prevLength = (lastObject.StartTime - lastLastObject.StartTime) / clockRate;

            Rhythm = getClosestRhythm(DeltaTime / prevLength);
            HitType = currentHit?.Type;

            ObjectIndex = objectIndex;
        }

        private static readonly TaikoDifficultyHitObjectRhythm[] common_rhythms =
        {
            new TaikoDifficultyHitObjectRhythm(1, 1, 0.0),
            new TaikoDifficultyHitObjectRhythm(2, 1, 0.3),
            new TaikoDifficultyHitObjectRhythm(1, 2, 0.5),
            new TaikoDifficultyHitObjectRhythm(3, 1, 0.3),
            new TaikoDifficultyHitObjectRhythm(1, 3, 0.35),
            new TaikoDifficultyHitObjectRhythm(3, 2, 0.6),
            new TaikoDifficultyHitObjectRhythm(2, 3, 0.4),
            new TaikoDifficultyHitObjectRhythm(5, 4, 0.5),
            new TaikoDifficultyHitObjectRhythm(4, 5, 0.7)
        };

        private TaikoDifficultyHitObjectRhythm getClosestRhythm(double ratio)
            => common_rhythms.OrderBy(x => Math.Abs(x.Ratio - ratio)).First();
    }
}
