// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A <see cref="HitObject"/> that appears on screen at a fixed time interval before its <see cref="HitObject.StartTime"/>.
    /// </summary>
    public interface IHasTimePreempt
    {
        double TimePreempt { get; }
    }
}
