// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Scoring;
using osu.Game.Screens.Multi.Ranking.Pages;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Multi.Ranking.Types
{
    public class RoomLeaderboardPageInfo : IResultPageInfo
    {
        private readonly ScoreInfo score;
        private readonly WorkingBeatmap beatmap;

        public RoomLeaderboardPageInfo(ScoreInfo score, WorkingBeatmap beatmap)
        {
            this.score = score;
            this.beatmap = beatmap;
        }

        public IconUsage Icon => FontAwesome.Solid.Users;

        public string Name => "Room Leaderboard";

        public virtual ResultsPage CreatePage() => new RoomLeaderboardPage(score, beatmap);
    }
}
