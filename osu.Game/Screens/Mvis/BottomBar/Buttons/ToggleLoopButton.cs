using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Screens.Mvis.BottomBar.Buttons
{
    public class ToggleLoopButton : BottomBarSwitchButton
    {
        public ToggleLoopButton()
        {
            ButtonIcon = FontAwesome.Solid.Undo;
            TooltipText = "单曲循环";
        }

        protected override void OnToggledOnAnimation()
        {
            base.OnToggledOnAnimation();

            SpriteIcon.RotateTo(0).RotateTo(-360, 1000, Easing.OutQuint);
        }
    }
}
