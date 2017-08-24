using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    internal class ToolbarDirectButton : ToolbarOverlayToggleButton
    {
        public ToolbarDirectButton()
        {
            SetIcon(FontAwesome.fa_download);
        }

        [BackgroundDependencyLoader]
        private void load(DirectOverlay direct)
        {
            StateContainer = direct;
        }
    }
}