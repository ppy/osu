// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.SearchableList
{
    public class HeaderTabControl<T> : OsuTabControl<T>
    {
        protected override TabItem<T> CreateTabItem(T value) => new HeaderTabItem(value);

        public HeaderTabControl()
        {
            Height = 26;
            AccentColour = Color4.White;
        }

        private class HeaderTabItem : OsuTabItem
        {
            public HeaderTabItem(T value) : base(value)
            {
                Text.TextSize = 16;
            }
        }
    }
}
