// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;

namespace osu.Game.Graphics.UserInterfaceV2
{
    /// <summary>
    /// A variant of <see cref="OsuDropdown{T}"/> that uses the nearest <see cref="OverlayColourProvider"/> for theming purposes, if one is available.
    /// </summary>
    public class ThemedDropdown<T> : OsuDropdown<T>
    {
        [BackgroundDependencyLoader(true)]
        private void load(OverlayColourProvider? colourProvider)
        {
            if (colourProvider == null) return;

            AccentColour = colourProvider.Light4;
        }

        protected override DropdownHeader CreateHeader() => new ThemedDropdownHeader();

        protected override DropdownMenu CreateMenu() => new ThemedDropdownMenu();

        protected class ThemedDropdownMenu : OsuDropdownMenu
        {
            [BackgroundDependencyLoader(true)]
            private void load(OverlayColourProvider? colourProvider)
            {
                if (colourProvider == null) return;

                BackgroundColour = colourProvider.Background5;
                ((IHasAccentColour)ContentContainer).AccentColour = colourProvider.Highlight1;
            }
        }

        protected class ThemedDropdownHeader : OsuDropdownHeader
        {
            [BackgroundDependencyLoader(true)]
            private void load(OverlayColourProvider? colourProvider)
            {
                if (colourProvider == null) return;

                BackgroundColour = colourProvider.Background5;
                BackgroundColourHover = colourProvider.Light4;
            }
        }
    }
}
