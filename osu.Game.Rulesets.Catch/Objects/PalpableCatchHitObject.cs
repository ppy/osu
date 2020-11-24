// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Catch.Objects
{
    /// <summary>
    /// Represents a single object that can be caught by the catcher.
    /// </summary>
    public abstract class PalpableCatchHitObject : CatchHitObject
    {
        public override bool CanBePlated => true;
    }
}
