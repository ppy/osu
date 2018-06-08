// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ScrollingInfo
    {
        public readonly ScrollingDirection Direction;

        public ScrollingInfo(ScrollingDirection direction)
        {
            Direction = direction;
        }
    }
}
