// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Login
{
    public partial class UserDropdown : OsuEnumDropdown<UserAction>
    {
        protected override DropdownHeader CreateHeader() => new UserDropdownHeader();

        protected override DropdownMenu CreateMenu() => new UserDropdownMenu();

        public Color4 StatusColour
        {
            set
            {
                if (Header is UserDropdownHeader h)
                    h.StatusColour = value;
            }
        }

        protected partial class UserDropdownMenu : OsuDropdownMenu
        {
            protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item) => new DrawableUserDropdownMenuItem(item);

            private partial class DrawableUserDropdownMenuItem : DrawableOsuDropdownMenuItem
            {
                public DrawableUserDropdownMenuItem(MenuItem item)
                    : base(item)
                {
                    Foreground.Padding = new MarginPadding { Top = 5, Bottom = 5, Left = 10, Right = 5 };
                }
            }
        }

        private partial class UserDropdownHeader : OsuDropdownHeader
        {
            private readonly StatusIcon statusIcon;

            public Color4 StatusColour
            {
                set => statusIcon.FadeColour(value, 500, Easing.OutQuint);
            }

            public UserDropdownHeader()
            {
                Foreground.Add(statusIcon = new StatusIcon
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Size = new Vector2(14),
                });

                Text.Margin = new MarginPadding { Left = 20 };
            }
        }
    }
}
