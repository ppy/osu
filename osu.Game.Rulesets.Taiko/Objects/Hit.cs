// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class Hit : TaikoStrongableHitObject
    {
        protected override StrongNestedHitObject CreateStrongNestedHit(double startTime) => new StrongNestedHit { StartTime = startTime };

        public class StrongNestedHit : StrongNestedHitObject
        {
        }
    }
}
