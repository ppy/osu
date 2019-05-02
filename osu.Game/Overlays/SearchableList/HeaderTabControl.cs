// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;
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
            public HeaderTabItem(T value)
                : base(value)
            {
                Text.Font = Text.Font.With(size: 16);
            }
        }
    }
}
