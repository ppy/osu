// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Screens.Mvis.Buttons
{
    public class ToggleableOverlayLockButton : ToggleableButton
    {
        public ToggleableOverlayLockButton()
        {
            ButtonIcon = FontAwesome.Solid.Lock;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            this.Delay(1000).FadeOut(500, Easing.OutQuint);
        }
    }
}
