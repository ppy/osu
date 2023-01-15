// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Beatmaps.ControlPoints
{
    public interface IControlPoint
    {
        /// <summary>
        /// The time at which the control point takes effect.
        /// </summary>
        double Time { get; }
    }
}
