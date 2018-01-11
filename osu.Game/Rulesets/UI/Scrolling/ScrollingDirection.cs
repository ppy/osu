// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.UI.Scrolling
{
    public enum ScrollingDirection
    {
        /// <summary>
        /// Hitobjects will scroll vertically from the bottom of the hitobject container.
        /// </summary>
        Up,
        /// <summary>
        /// Hitobjects will scroll vertically from the top of the hitobject container.
        /// </summary>
        Down,
        /// <summary>
        /// Hitobjects will scroll horizontally from the right of the hitobject container.
        /// </summary>
        Left,
        /// <summary>
        /// Hitobjects will scroll horizontally from the left of the hitobject container.
        /// </summary>
        Right
    }
}
