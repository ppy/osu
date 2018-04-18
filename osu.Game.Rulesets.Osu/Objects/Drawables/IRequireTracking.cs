// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
