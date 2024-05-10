// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Input.Bindings;

namespace osu.Game.Screens.Select.FooterV2
{
    public partial class FooterButtonOptionsV2 : FooterButtonV2, IHasPopover
    {
        /// <summary>
        /// True if the next click is for hiding the popover.
        /// </summary>
        private bool hidingFromClick;

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            Text = "Options";
            Icon = FontAwesome.Solid.Cog;
            AccentColour = colour.Purple1;
            Hotkey = GlobalAction.ToggleBeatmapOptions;

            Action = () =>
            {
                if (OverlayState.Value == Visibility.Hidden && !hidingFromClick)
                    this.ShowPopover();

                hidingFromClick = false;
            };
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (OverlayState.Value == Visibility.Visible)
                hidingFromClick = true;

            return base.OnMouseDown(e);
        }

        protected override void Flash()
        {
            if (hidingFromClick)
                return;

            base.Flash();
        }

        public Popover GetPopover() => new BeatmapOptionsPopover(this);
    }
}
