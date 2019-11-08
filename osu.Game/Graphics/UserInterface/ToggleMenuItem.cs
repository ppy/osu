// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public class ToggleMenuItem : StatefulMenuItem<bool>
    {
        public ToggleMenuItem(string text, MenuItemType type = MenuItemType.Standard)
            : this(text, type, null)
        {
        }

        public ToggleMenuItem(string text, MenuItemType type, Action<bool> action)
            : base(text, type, value => !value)
        {
            State.BindValueChanged(state => action?.Invoke(state.NewValue));
        }

        public override IconUsage? GetIconForState(bool state) => state ? (IconUsage?)FontAwesome.Solid.Check : null;
    }
}
