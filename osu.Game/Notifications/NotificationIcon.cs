// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Colour;
using osu.Game.Graphics;

namespace osu.Game.Notifications
{
    public class NotificationIcon
    {

        /// <summary>
        /// The icon displayed in the notification.
        /// </summary>
        public FontAwesome Icon { get; }

        /// <summary>
        /// Changes to background color of the icon. Defaults to grey
        /// </summary>
        public ColourInfo BackgroundColour { get; }

        public NotificationIcon(FontAwesome icon = FontAwesome.fa_info_circle, ColourInfo? backgroundColour = null)
        {
            Icon = icon;
            BackgroundColour = backgroundColour ?? ColourInfo.GradientVertical(OsuColour.Gray(0.2f), OsuColour.Gray(0.6f));
        }
    }
}
