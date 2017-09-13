// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Menus
{
    public class EditorMenuItem : OsuMenuItem
    {
        private const int min_text_length = 40;

        public EditorMenuItem(string text, MenuItemType type = MenuItemType.Standard)
            : base(text.PadRight(min_text_length), type)
        {
        }

        public EditorMenuItem(string text, MenuItemType type, Action action)
            : base(text.PadRight(min_text_length), type, action)
        {
        }
    }
}
