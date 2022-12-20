// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Overlays.Mf.Header
{
    public partial class MfMenuHeader : OverlayHeader
    {
        protected override OverlayTitle CreateTitle() => new MfTitle();

        private partial class MfTitle : OverlayTitle
        {
            public MfTitle()
            {
                Title = "关于Mf-osu";
                Description = "这是基于osu!lazer的一个分支版本,祝游玩愉快(｡･ω･)ﾉﾞ";
                IconTexture = "Icons/Hexacons/aboutMF";
            }
        }
    }
}
