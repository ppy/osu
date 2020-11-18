using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public class PlayerSettingsDropdown<T> : OsuEnumDropdown<T>
        where T : struct, Enum
    {
        protected override DropdownHeader CreateHeader() => new PlayerSettingsDropdownHeader();

        protected override DropdownMenu CreateMenu() => new PlayerSettingsDropdownMenu();

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.Yellow;
        }

        private class PlayerSettingsDropdownHeader : OsuDropdownHeader
        {
            public PlayerSettingsDropdownHeader()
            {
                Height = 25;
                Foreground.Padding = new MarginPadding { Top = 4, Bottom = 4, Left = 8, Right = 8 };
            }
        }

        private class PlayerSettingsDropdownMenu : OsuDropdownMenu
        {
        }
    }
}
