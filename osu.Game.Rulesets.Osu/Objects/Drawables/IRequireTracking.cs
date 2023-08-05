// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public interface IRequireTracking
    {
        /// <summary>
        /// Whether the <see cref="DrawableSlider"/> is currently being tracked by the user.
        /// </summary>
        bool Tracking { get; set; }
    }
}
