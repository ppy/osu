// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Overlays.MfMenu
{
    public class MfMenuHeader : OverlayHeader
    {
        protected override OverlayTitle CreateTitle() => new MfTitle();

        private class MfTitle : OverlayTitle
        {
            public MfTitle()
            {
                Title = "关于Mf-osu";
                Description = "这是一个官方lazer的分支,祝游玩愉快(｡･ω･)ﾉﾞ";
                IconTexture = "Icons/Hexacons/contests";
            }
        }
    }
}
