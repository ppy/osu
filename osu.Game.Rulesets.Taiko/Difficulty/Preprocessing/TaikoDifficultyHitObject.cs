// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    public class TaikoDifficultyHitObject : DifficultyHitObject
    {
        public readonly bool HasTypeChange;

        public TaikoDifficultyHitObject(HitObject hitObject, HitObject lastObject, double clockRate)
            : base(hitObject, lastObject, clockRate)
        {
            HasTypeChange = lastObject is RimHit != hitObject is RimHit;
        }
    }
}
