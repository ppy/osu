// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.Edit.Screens.Setup.Components.LabelledBoxes
{
    // TODO: Uncomment constraint once upgraded to C# 7.3 or greater
    public class LabelledEnumDropdown<T> : LabelledDropdown<T>
        //where T : Enum
    {
        protected override OsuDropdown<T> CreateDropdown()
        {
            return new OsuEnumDropdown<T>
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                HeaderHeight = DEFAULT_HEIGHT,
                HeaderCornerRadius = INNER_CORNER_RADIUS,
                HeaderTextSize = DEFAULT_HEADER_TEXT_SIZE,
                HeaderTextLeftPadding = DEFAULT_PADDING,
                HeaderDownIconRightPadding = DEFAULT_PADDING,
            };
        }
    }
}
