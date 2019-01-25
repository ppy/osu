// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Scoring;
using osu.Game.Screens.Multi.Ranking.Pages;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Multi.Ranking.Types
{
    public class RoomLeaderboardPageInfo : IResultPageInfo
    {
        private readonly ScoreInfo score;
        private readonly WorkingBeatmap beatmap;
        private readonly Room room;

        public RoomLeaderboardPageInfo(ScoreInfo score, WorkingBeatmap beatmap, Room room)
        {
            this.score = score;
            this.beatmap = beatmap;
            this.room = room;
        }

        public FontAwesome Icon => FontAwesome.fa_users;

        public string Name => "Room Leaderboard";

        public virtual ResultsPage CreatePage() => new RoomLeaderboardPage(score, beatmap, room);
    }
}
