// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    /// <summary>
    /// A component which tracks the current end snaking position of a slider.
    /// </summary>
    public interface ITrackSnaking
    {
        void UpdateSnakingPosition(Vector2 start, Vector2 end);
    }
}
