// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;

namespace osu.Game.Graphics.UserInterfaceV2
{
    /// <summary>
    /// A variant of <see cref="OsuDropdown{T}"/> that uses the nearest <see cref="OverlayColourProvider"/> for theming purposes.
    /// </summary>
    public class ThemedDropdown<T> : OsuDropdown<T>
    {
        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            AccentColour = colourProvider.Light4;
        }

        protected override DropdownHeader CreateHeader() => new ThemedDropdownHeader();

        protected override DropdownMenu CreateMenu() => new ThemedDropdownMenu();

        protected class ThemedDropdownMenu : OsuDropdownMenu
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                BackgroundColour = colourProvider.Background5;
            }
        }

        protected class ThemedDropdownHeader : OsuDropdownHeader
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                BackgroundColour = colourProvider.Background5;
                BackgroundColourHover = colourProvider.Light4;
            }
        }
    }
}
