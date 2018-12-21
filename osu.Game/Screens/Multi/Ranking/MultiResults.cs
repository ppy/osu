// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Online.Multiplayer;
using osu.Game.Scoring;
using osu.Game.Screens.Multi.Ranking.Types;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Ranking.Types;

namespace osu.Game.Screens.Multi.Ranking
{
    public class MultiResults : Results
    {
        private readonly Room room;

        public MultiResults(ScoreInfo score, Room room)
            : base(score)
        {
            this.room = room;
        }

        protected override IEnumerable<IResultType> CreateResultTypes() => new IResultType[]
        {
            new ScoreResultType(Score, Beatmap),
            new RankingResultType(Score, Beatmap),
            new RoomRankingResultType(Score, Beatmap, room),
        };
    }
}
