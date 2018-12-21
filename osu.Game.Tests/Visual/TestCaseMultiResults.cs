// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Scoring;
using osu.Game.Screens.Multi.Ranking;
using osu.Game.Screens.Multi.Ranking.Pages;
using osu.Game.Screens.Multi.Ranking.Types;
using osu.Game.Users;

namespace osu.Game.Tests.Visual
{
    public class TestCaseMultiResults : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(MultiResults),
            typeof(RoomRankingResultType),
            typeof(RoomRankingResultsPage)
        };

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            var beatmapInfo = beatmaps.QueryBeatmap(b => b.RulesetID == 0);
            if (beatmapInfo != null)
                Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmapInfo);

            MultiResults res;

            Child = res = new MultiResults(new ScoreInfo
            {
                User = new User
                {
                    Id = 9623649
                },
            }, new Room { RoomID = { Value = 46 } });
        }
    }
}
