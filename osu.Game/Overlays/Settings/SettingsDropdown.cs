// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings
{
    public class SettingsDropdown<T> : SettingsItem<T>
    {
        protected new OsuDropdown<T> Control => (OsuDropdown<T>)base.Control;

        public IEnumerable<T> Items
        {
            get => Control.Items;
            set => Control.Items = value;
        }

        public IBindableList<T> ItemSource
        {
            get => Control.ItemSource;
            set => Control.ItemSource = value;
        }

        public override IEnumerable<string> FilterTerms => base.FilterTerms.Concat(Control.Items.Select(i => i.ToString()));

        protected sealed override Drawable CreateControl() => CreateDropdown();

        protected virtual OsuDropdown<T> CreateDropdown() => new DropdownControl();

        protected class DropdownControl : OsuDropdown<T>
        {
            public DropdownControl()
            {
                Margin = new MarginPadding { Top = 5 };
                RelativeSizeAxes = Axes.X;
            }
        }
    }
}
