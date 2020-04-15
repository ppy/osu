// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuMenuItem : MenuItem
    {
        public readonly Bindable<bool> Enabled = new Bindable<bool>(true);

        public readonly MenuItemType Type;

        public OsuMenuItem(string text, MenuItemType type = MenuItemType.Standard)
            : this(text, type, null)
        {
        }

        public OsuMenuItem(string text, MenuItemType type, Action action)
            : base(text, action)
        {
            Type = type;

            Enabled.BindValueChanged(enabled => Action.Disabled = !enabled.NewValue);
            Action.BindDisabledChanged(disabled => Enabled.Value = !disabled);
        }
    }
}
