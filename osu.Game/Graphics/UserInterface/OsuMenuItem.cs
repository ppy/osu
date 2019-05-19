// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Commands;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuMenuItem : MenuItem
    {
        public readonly MenuItemType Type;

        public OsuMenuItem(string text, MenuItemType type = MenuItemType.Standard)
            : base(text)
        {
            Type = type;
        }

        public OsuMenuItem(string text, MenuItemType type, ICommand command)
            : base(text, command)
        {
            Type = type;
        }
    }
}
