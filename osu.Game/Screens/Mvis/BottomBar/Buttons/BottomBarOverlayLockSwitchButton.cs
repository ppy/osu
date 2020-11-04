// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Screens.Mvis.BottomBar.Buttons
{
    public class BottomBarOverlayLockSwitchButton : BottomBarSwitchButton
    {
        private float DURATION = 100;
        public BottomBarOverlayLockSwitchButton()
        {
            ButtonIcon = FontAwesome.Solid.Lock;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            this.Delay(1000).FadeOut(500, Easing.OutQuint);
        }

        protected override void OnToggledOnAnimation()
        {
            base.OnToggledOnAnimation();

            spriteIcon.RotateTo(15, DURATION).Then()
                      .RotateTo(-15, DURATION).Loop(0, 2).Then()
                      .RotateTo(0, DURATION);
        }
    }
}
