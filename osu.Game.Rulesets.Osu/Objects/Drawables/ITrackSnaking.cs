// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;

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
