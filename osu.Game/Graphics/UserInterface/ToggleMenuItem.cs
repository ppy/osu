// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;

namespace osu.Game.Graphics.UserInterface
{
    public class ToggleMenuItem : OsuMenuItem
    {
        public readonly BindableBool State = new BindableBool();

        public ToggleMenuItem(string text, MenuItemType type = MenuItemType.Standard)
            : this(text, type, null)
        {
        }

        public ToggleMenuItem(string text, MenuItemType type, Action<bool> action)
            : base(text, type)
        {
            Action.Value = () => State.Toggle();
            State.BindValueChanged(state => action?.Invoke(state.NewValue));
        }
    }
}
