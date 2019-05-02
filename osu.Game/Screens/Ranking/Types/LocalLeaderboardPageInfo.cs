// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Pages;

namespace osu.Game.Screens.Ranking.Types
{
    public class LocalLeaderboardPageInfo : IResultPageInfo
    {
        private readonly ScoreInfo score;
        private readonly WorkingBeatmap beatmap;

        public LocalLeaderboardPageInfo(ScoreInfo score, WorkingBeatmap beatmap)
        {
            this.score = score;
            this.beatmap = beatmap;
        }

        public IconUsage Icon => FontAwesome.Solid.User;

        public string Name => @"Local Leaderboard";

        public ResultsPage CreatePage() => new LocalLeaderboardPage(score, beatmap);
    }
}
