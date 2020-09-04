// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Overlays.Dashboard
{
    public class DashboardOverlayHeader : TabControlOverlayHeaderCN<DashboardOverlayTabs>
    {
        protected override OverlayTitle CreateTitle() => new DashboardTitle();

        private class DashboardTitle : OverlayTitle
        {
            public DashboardTitle()
            {
                Title = "看板";
                Description = "在这里查看好友和其他的信息";
                IconTexture = "Icons/Hexacons/social";
            }
        }
    }

    public enum DashboardOverlayTabs
    {
        [Description("好友列表")]
        Friends
    }
}
