// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Input.Bindings;

namespace osu.Game.Screens.SelectV2.Footer
{
    public partial class FooterButtonOptionsV2 : FooterButtonV2, IHasPopover
    {
        public readonly BindableBool IsActive = new BindableBool();

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            Text = "Options";
            Icon = FontAwesome.Solid.Cog;
            AccentColour = colour.Purple1;
            Hotkey = GlobalAction.ToggleBeatmapOptions;

            Action = () => IsActive.Toggle();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            IsActive.BindValueChanged(active =>
            {
                OverlayState.Value = active.NewValue ? Visibility.Visible : Visibility.Hidden;
            });

            OverlayState.BindValueChanged(state =>
            {
                switch (state.NewValue)
                {
                    case Visibility.Hidden:
                        this.HidePopover();
                        break;

                    case Visibility.Visible:
                        this.ShowPopover();
                        break;
                }
            });
        }

        public Popover GetPopover() => new BeatmapOptionsPopover(this);
    }
}
