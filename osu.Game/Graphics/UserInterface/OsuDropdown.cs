// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuDropdown<T> : Dropdown<T>
    {
        protected override DropdownHeader CreateHeader() => new OsuDropdownHeader { AccentColour = AccentColour };

        protected override Menu CreateMenu() => new OsuMenu();

        private Color4? accentColour;
        public virtual Color4 AccentColour
        {
            get { return accentColour.GetValueOrDefault(); }
            set
            {
                accentColour = value;
                if (Header != null)
                    ((OsuDropdownHeader)Header).AccentColour = value;
                foreach (var item in MenuItems.OfType<OsuDropdownMenuItem<T>>())
                    item.AccentColour = value;
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (accentColour == null)
                AccentColour = colours.PinkDarker;
        }

        protected override DropdownMenuItem<T> CreateMenuItem(string text, T value) => new OsuDropdownMenuItem<T>(text, value) { AccentColour = AccentColour };
    }
}