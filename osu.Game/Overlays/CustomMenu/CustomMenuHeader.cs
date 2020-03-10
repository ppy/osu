// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.CustomMenu
{
    public class CustomMenuHeader : OverlayHeader
    {
        protected override ScreenTitle CreateTitle() => new CustomMenuTitle();

        private class CustomMenuTitle : ScreenTitle
        {
            public CustomMenuTitle()
            {
                Title = @"测试菜单";
                Section = @"菜单";
            }
        }
    }
}
