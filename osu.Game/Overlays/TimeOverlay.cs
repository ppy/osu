using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays
{
    //这是一个占位的，后续会进行开发
    public class TimeOverlay : OsuFocusedOverlayContainer, INamedOverlayComponent
    {
        public string IconTexture => "Icons/Hexacons/calendar";
        public string Title => "时间";

        public string Description => "如果你看到了这个描述，那你很可能是遇到bug了";

        protected override void PopIn()
        {
            this.Hide();
        }
    }
}