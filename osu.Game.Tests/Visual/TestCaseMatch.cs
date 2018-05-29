// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Screens.Match;

namespace osu.Game.Tests.Visual
{
    public class TestCaseMatch : OsuTestCase
    {
        public TestCaseMatch()
        {
            Room room = new Room
            {
                Beatmap =
                {
                    Value = new BeatmapInfo
                    {
                        BeatmapSet = new BeatmapSetInfo
                        {
                            OnlineInfo = new BeatmapSetOnlineInfo
                            {
                                Covers = new BeatmapSetOnlineCovers
                                {
                                    Cover = @"https://assets.ppy.sh/beatmaps/765055/covers/cover.jpg?1526955337",
                                },
                            },
                        },
                    },
                },
            };

            Match match = new Match(room);

            AddStep(@"show", () => Add(match));
            AddStep(@"exit", match.Exit);
        }
    }
}
