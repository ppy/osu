// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Screens.Multi.Screens.Match;

namespace osu.Game.Tests.Visual
{
    public class TestCaseMatch : OsuTestCase
    {
        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            Room room = new Room
            {
                Name = { Value = @"One Awesome Room" },
                Status = { Value = new RoomStatusOpen() },
                Availability = { Value = RoomAvailability.Public },
                Type = { Value = new GameTypeTeamVersus() },
                Beatmap =
                {
                    Value = new BeatmapInfo
                    {
                        StarDifficulty = 5.02,
                        Ruleset = rulesets.GetRuleset(1),
                        Metadata = new BeatmapMetadata
                        {
                            Title = @"Paradigm Shift",
                            Artist = @"Morimori Atsushi",
                            AuthorString = @"eiri-",
                        },
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
