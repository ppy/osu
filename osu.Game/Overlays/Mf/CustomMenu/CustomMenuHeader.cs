// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.CustomMenu
{
    public class CustomMenuHeader : OverlayHeader
    {
        protected override Drawable CreateBackground() => new OverlayHeaderBackground(@"Headers/search");
        protected override ScreenTitle CreateTitle() => new CustomMenuTitle();

        private class CustomMenuTitle : ScreenTitle
        {
            public CustomMenuTitle()
            {
                Title = @"关于Mf-osu";
                Section = @"页面";
            }

            protected override Drawable CreateIcon() => new ScreenTitleTextureIcon(@"Icons/news");

        }
    }
}
