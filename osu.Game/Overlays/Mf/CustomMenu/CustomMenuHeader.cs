// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Overlays.MfMenu
{
    public class MfMenuHeader : OverlayHeader
    {
        protected override Drawable CreateBackground() => new OverlayHeaderBackground(@"Headers/search");
        protected override OverlayTitle CreateTitle() => new MfMenuTitle();

        private class MfMenuTitle : OverlayTitle
        {
            public MfMenuTitle()
            {
                Title = "关于Mf-osu 页面";
                IconTexture = "Icons/news";
            }

        }
    }
}
