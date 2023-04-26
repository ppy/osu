// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuMenuItem : MenuItem
    {
        public readonly MenuItemType Type;

        public OsuMenuItem(LocalisableString text, MenuItemType type = MenuItemType.Standard)
            : this(text, type, null)
        {
        }

        public OsuMenuItem(LocalisableString text, MenuItemType type, Action? action)
            : base(text, action)
        {
            Type = type;
        }
    }
}
