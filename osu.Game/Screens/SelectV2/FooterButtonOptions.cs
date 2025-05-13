// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Screens.Footer;

namespace osu.Game.Screens.SelectV2
{
    public partial class FooterButtonOptions : ScreenFooterButton, IHasPopover
    {
        private readonly SongSelect songSelect;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private IBindable<WorkingBeatmap> beatmap = null!;

        public FooterButtonOptions(SongSelect songSelect)
        {
            this.songSelect = songSelect;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            Text = "Options";
            Icon = FontAwesome.Solid.Cog;
            AccentColour = colour.Purple1;
            Hotkey = GlobalAction.ToggleBeatmapOptions;

            Action = this.ShowPopover;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap = songSelect.Beatmap.GetBoundCopy();
            beatmap.BindValueChanged(_ => beatmapChanged(), true);
        }

        private void beatmapChanged()
        {
            this.HidePopover();
            Enabled.Value = !beatmap.IsDefault;
        }

        public Framework.Graphics.UserInterface.Popover GetPopover() => new Popover(this, beatmap.Value, songSelect, colourProvider);
    }
}
