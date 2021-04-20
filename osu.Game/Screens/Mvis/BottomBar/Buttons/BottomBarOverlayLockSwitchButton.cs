using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.BottomBar.Buttons
{
    public class BottomBarOverlayLockSwitchButton : BottomBarSwitchButton
    {
        private const float animate_duration = 100;
        private bool disabled;

        public bool Disabled
        {
            get => disabled;
            set
            {
                this.FadeColour(value ? Color4.Gray : Color4.White, 300, Easing.OutQuint);

                disabled = value;
            }
        }

        public BottomBarOverlayLockSwitchButton()
        {
            ButtonIcon = FontAwesome.Solid.Lock;
        }

        [Resolved]
        private MvisScreen mvisScreen { get; set; }

        protected override void OnToggledOnAnimation()
        {
            base.OnToggledOnAnimation();

            SpriteIcon.RotateTo(15, animate_duration).Then()
                      .RotateTo(-15, animate_duration).Loop(0, 2).Then()
                      .RotateTo(0, animate_duration);
        }
    }
}
