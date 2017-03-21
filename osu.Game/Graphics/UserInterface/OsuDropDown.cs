// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuDropDown<T> : DropDown<T>
    {
        protected override DropDownHeader CreateHeader() => new OsuDropDownHeader { AccentColour = AccentColour };

        protected override Menu CreateMenu() => new OsuMenu();

        private Color4? accentColour;
        public virtual Color4 AccentColour
        {
            get { return accentColour.GetValueOrDefault(); }
            set
            {
                accentColour = value;
                if (Header != null)
                    ((OsuDropDownHeader)Header).AccentColour = value;
                foreach (var item in MenuItems.OfType<OsuDropDownMenuItem<T>>())
                    item.AccentColour = value;
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (accentColour == null)
                AccentColour = colours.PinkDarker;
        }

        protected override DropDownMenuItem<T> CreateMenuItem(string key, T value) => new OsuDropDownMenuItem<T>(key, value) { AccentColour = AccentColour };
    }
}