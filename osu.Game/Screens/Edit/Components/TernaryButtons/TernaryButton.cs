// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Components.TernaryButtons
{
    public class TernaryButton
    {
        public readonly Bindable<TernaryState> Bindable;

        public readonly string Description;

        /// <summary>
        /// A function which creates a drawable icon to represent this item. If null, a sane default should be used.
        /// </summary>
        public readonly Func<Drawable>? CreateIcon;

        public TernaryButton(Bindable<TernaryState> bindable, string description, Func<Drawable>? createIcon = null)
        {
            Bindable = bindable;
            Description = description;
            CreateIcon = createIcon;
        }

        public void Toggle()
        {
            switch (Bindable.Value)
            {
                case TernaryState.False:
                case TernaryState.Indeterminate:
                    Bindable.Value = TernaryState.True;
                    break;

                case TernaryState.True:
                    Bindable.Value = TernaryState.False;
                    break;
            }
        }
    }
}
