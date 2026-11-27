// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Rankings
{
    public partial class RankingSelectorDropdown<T> : OsuDropdown<T>
        where T : class
    {
        private OsuDropdownMenu menu = null!;

        protected override DropdownMenu CreateMenu() => menu = (OsuDropdownMenu)base.CreateMenu().With(m => m.MaxHeight = 400);

        protected override DropdownHeader CreateHeader() => new RankingSelectorDropdownHeader();

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            menu.BackgroundColour = colourProvider.Background5;
            menu.HoverColour = colourProvider.Background4;
            menu.SelectionColour = colourProvider.Background3;
        }

        private partial class RankingSelectorDropdownHeader : OsuDropdownHeader
        {
            public RankingSelectorDropdownHeader()
            {
                AutoSizeAxes = Axes.Y;
                Text.Font = OsuFont.GetFont(size: 15);
                Text.Padding = new MarginPadding { Vertical = 1.5f }; // osu-web line-height difference compensation
                Foreground.Padding = new MarginPadding { Horizontal = 10, Vertical = 15 };
                Margin = Chevron.Margin = new MarginPadding(0);
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                BackgroundColour = colourProvider.Background6.Opacity(0.5f);
                // osu-web adds a 0.6 opacity container on top of the 0.5 base one when hovering, 0.8 on a single container here matches the resulting colour
                BackgroundColourHover = colourProvider.Background6.Opacity(0.8f);
            }
        }
    }
}
