// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Dashboard
{
    public class DashboardOverlayHeader : TabControlOverlayHeader<HomeOverlayTabs>
    {
        protected override ScreenTitle CreateTitle() => new DashboardTitle();

        private class DashboardTitle : ScreenTitle
        {
            public DashboardTitle()
            {
                Title = "看板";
            }

            protected override Drawable CreateIcon() => new ScreenTitleTextureIcon(@"Icons/changelog");
        }
    }

    public enum HomeOverlayTabs
    {
        [Description("好友列表")]
        Friends
    }
}