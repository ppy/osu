// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Online.Multiplayer;
using osu.Game.Scoring;
using osu.Game.Screens.Multi.Ranking.Types;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Ranking.Types;

namespace osu.Game.Screens.Multi.Ranking
{
    public class MatchResults : Results
    {
        private readonly Room room;

        public MatchResults(ScoreInfo score, Room room)
            : base(score)
        {
            this.room = room;
        }

        protected override IEnumerable<IResultPageInfo> CreateResultPages() => new IResultPageInfo[]
        {
            new ScoreOverviewPageInfo(Score, Beatmap),
            new LocalLeaderboardPageInfo(Score, Beatmap),
            new RoomLeaderboardPageInfo(Score, Beatmap, room),
        };
    }
}
