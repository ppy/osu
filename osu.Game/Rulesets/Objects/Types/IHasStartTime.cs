// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// This object has a start time.
    /// </summary>
    public interface IHasStartTime
    {
        double StartTime { get; set; }
    }
}
