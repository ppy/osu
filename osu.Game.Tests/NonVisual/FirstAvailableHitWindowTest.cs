// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Tests.NonVisual
{
    public class TestHitObject : HitObject
    {
        public TestHitObject(HitWindows hitWindows)
        {
            HitWindows = hitWindows;
            HitWindows.SetDifficulty(0.5f);
        }

        public new void AddNested(HitObject nested) => base.AddNested(nested);
    }
}
