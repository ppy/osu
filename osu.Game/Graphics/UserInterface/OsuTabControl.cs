// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.UserInterface.Tab;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuTabControl<T> : TabControl<T>
    {
        protected override TabDropDownMenu<T> CreateDropDownMenu() => new OsuTabDropDownMenu<T>();

        protected override TabItem<T> CreateTabItem(T value) => new OsuTabItem<T> { Value = value };

        public OsuTabControl()
        {
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException("OsuTabControl only supports enums as the generic type argument");

            foreach (var val in (T[])Enum.GetValues(typeof(T)))
                AddTab(val);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (accentColour == null)
                AccentColour = colours.Blue;
        }

        private Color4? accentColour;
        public Color4 AccentColour
        {
            get { return accentColour.GetValueOrDefault(); }
            set
            {
                accentColour = value;
                var dropDown = DropDown as OsuTabDropDownMenu<T>;
                if (dropDown != null)
                    dropDown.AccentColour = value;
                foreach (var item in TabContainer.Children.OfType<OsuTabItem<T>>())
                    item.AccentColour = value;
            }
        }
    }
}
