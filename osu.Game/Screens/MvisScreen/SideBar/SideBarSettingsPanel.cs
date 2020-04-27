using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Mvis.SideBar
{
    public class SideBarSettingsPanel : VisibilityContainer
    {
        private const float DURATION = 750;

        public SideBarSettingsPanel()
        {
        }

        protected override void PopIn()
        {
            this.MoveToX(0, DURATION, Easing.OutQuint).FadeIn(DURATION, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.MoveToX(400, DURATION, Easing.OutQuint).FadeOut(DURATION, Easing.OutQuint);
        }
    }
}