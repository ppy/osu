// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mania.Objects
{
    /// <summary>
    /// The head note of a <see cref="HoldNote"/>.
    /// </summary>
    public class HeadNote : Note
    {
        protected override HitObject CreateInstance() => new HeadNote();
    }
}
