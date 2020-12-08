// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Overlays.Rankings
{
    public class RankingsSortTabControl : OverlaySortTabControl<RankingsSortCriteria>
    {
        public RankingsSortTabControl()
        {
            Title = "显示";
        }
    }

    public enum RankingsSortCriteria
    {
        [Description("所有")]
        All,

        [Description("仅好友")]
        Friends
    }
}
