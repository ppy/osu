// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Components.Menus
{
    public class EditorMenuItem : OsuMenuItem
    {
        public EditorMenuItem(LocalisableString text, MenuItemType type = MenuItemType.Standard)
            : base(text, type)
        {
        }

        public EditorMenuItem(LocalisableString text, MenuItemType type, Action action)
            : base(text, type, action)
        {
        }
    }
}
