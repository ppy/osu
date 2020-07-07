// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Screens.Multi.Components;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class OverlinedLeaderboard : OverlinedDisplay
    {
        private readonly MatchLeaderboard leaderboard;

        public OverlinedLeaderboard()
            : base("排行榜")
        {
            Content.Add(leaderboard = new MatchLeaderboard
            {
                RelativeSizeAxes = Axes.Both
            });
        }

        public void RefreshScores() => leaderboard.RefreshScores();
    }
}
