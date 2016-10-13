//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Overlays
{
    public enum Visibility
    {
        Hidden,
        Visible
    }

    public static class OverlayVisibilityHelper
    {
        public static Visibility Reverse(this Visibility input)
            => input == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
    }
}
