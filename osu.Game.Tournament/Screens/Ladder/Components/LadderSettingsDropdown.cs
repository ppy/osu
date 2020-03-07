// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class LadderSettingsDropdown<T> : SettingsDropdown<T>
    {
        protected override OsuDropdown<T> CreateDropdown() => new DropdownControl();

        private new class DropdownControl : SettingsDropdown<T>.DropdownControl
        {
            protected override DropdownMenu CreateMenu() => new Menu();

            private new class Menu : OsuDropdownMenu
            {
                public Menu()
                {
                    MaxHeight = 200;
                }
            }
        }
    }
}
