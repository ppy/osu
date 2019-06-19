// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays
{
    public class OverlayHeaderTabControl : OverlayTabControl<string>
    {
        protected override TabItem<string> CreateTabItem(string value) => new OverlayHeaderTabItem(value)
        {
            AccentColour = AccentColour,
        };

        private class OverlayHeaderTabItem : OverlayTabItem<string>
        {
            public OverlayHeaderTabItem(string value)
                : base(value)
            {
                Text.Text = value;
            }
        }
    }
}
