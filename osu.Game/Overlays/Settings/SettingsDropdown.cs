// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings
{
    public class SettingsDropdown<T> : SettingsItem<T>
    {
        private Dropdown<T> dropdown;

        private IEnumerable<KeyValuePair<string, T>> items = new KeyValuePair<string, T>[] { };
        public IEnumerable<KeyValuePair<string, T>> Items
        {
            get
            {
                return items;
            }
            set
            {
                items = value;
                if (dropdown != null)
                    dropdown.Items = value;
            }
        }

        protected override Drawable CreateControl() => dropdown = new OsuDropdown<T>
        {
            Margin = new MarginPadding { Top = 5 },
            RelativeSizeAxes = Axes.X,
            Items = Items,
        };
    }
}
