// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Screens.Setup.Components.LabelledBoxes
{
    // TODO: Uncomment constraint once upgraded to C# 7.3 or greater
    public class LabelledEnumDropdown<T> : LabelledDropdown<T>
        //where T : Enum
    {
        protected override OsuDropdown<T> CreateDropdown() => new OsuEnumDropdown<T>
        {
            Anchor = Anchor.TopLeft,
            Origin = Anchor.TopLeft,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            HeaderHeight = DEFAULT_HEIGHT,
            HeaderCornerRadius = INNER_CORNER_RADIUS,
            HeaderTextSize = DEFAULT_HEADER_TEXT_SIZE,
            HeaderTextLeftPadding = DEFAULT_HEADER_TEXT_PADDING,
            HeaderDownIconRightPadding = DEFAULT_HEADER_ICON_PADDING,
        };
    }
}
