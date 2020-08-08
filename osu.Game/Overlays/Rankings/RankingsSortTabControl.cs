// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Overlays.Rankings
{
    public class RankingsSortTabControl : OverlaySortTabControl<RankingsSortCriteria>
    {
        public RankingsSortTabControl()
        {
            Title = "Show";
        }
    }

    public enum RankingsSortCriteria
    {
        All,
        Friends
    }
}
