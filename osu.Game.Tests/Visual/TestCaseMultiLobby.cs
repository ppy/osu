// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Screens.Multiplayer;
using osu.Game.Users;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseMultiLobby : OsuTestCase
    {
        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            Lobby lobby;
            Add(lobby = new Lobby());

            AddStep(@"add rooms", () => lobby.Rooms = new[]
            {
                new Room
                {
                    Name = { Value = @"My Awesome Room" },
                    Host = { Value = new User { Username = @"flyte", Id = 3103765, Country = new Country { FlagName = @"JP" } } },
                    Status = { Value = new RoomStatusOpen() },
                    Availability = { Value = RoomAvailability.Public },
                    Type = { Value = new GameTypeTeamVersus() },
                    Beatmap =
                    {
                        Value = new BeatmapInfo
                        {
                            StarDifficulty = 3.7,
                            Ruleset = rulesets.GetRuleset(3),
                            Metadata = new BeatmapMetadata
                            {
                                Title = @"Platina",
                                Artist = @"Maaya Sakamoto",
                                AuthorString = @"uwutm8",
                            },
                            BeatmapSet = new BeatmapSetInfo
                            {
                                OnlineInfo = new BeatmapSetOnlineInfo
                                {
                                    Covers = new BeatmapSetOnlineCovers
                                    {
                                        Cover = @"https://assets.ppy.sh/beatmaps/560573/covers/cover.jpg?1492722343",
                                    },
                                },
                            },
                        },
                    },
                    MaxParticipants = { Value = 200 },
                    Participants =
                    {
                        Value = new[]
                        {
                            new User { Username = @"flyte", Id = 3103765, Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 142 } } },
                            new User { Username = @"Cookiezi", Id = 124493, Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 546 } } },
                            new User { Username = @"Angelsim", Id = 1777162, Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 287 } } },
                            new User { Username = @"Rafis", Id = 2558286, Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 468 } } },
                            new User { Username = @"hvick225", Id = 50265, Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 325 } } },
                            new User { Username = @"peppy", Id = 2, Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 625 } } },
                        },
                    },
                },
                new Room
                {
                    Name = { Value = @"My New, Better Than Ever Room" },
                    Host = { Value = new User { Username = @"Angelsim", Id = 1777162, Country = new Country { FlagName = @"KR" } } },
                    Status = { Value = new RoomStatusPlaying() },
                    Availability = { Value = RoomAvailability.Private },
                    Type = { Value = new GameTypeTagTeam() },
                    Beatmap =
                    {
                        Value = new BeatmapInfo
                        {
                            StarDifficulty = 7.07,
                            Ruleset = rulesets.GetRuleset(0),
                            Metadata = new BeatmapMetadata
                            {
                                Title = @"FREEDOM DIVE",
                                Artist = @"xi",
                                AuthorString = @"Nakagawa-Kanon",
                            },
                            BeatmapSet = new BeatmapSetInfo
                            {
                                OnlineInfo = new BeatmapSetOnlineInfo
                                {
                                    Covers = new BeatmapSetOnlineCovers
                                    {
                                        Cover = @"https://assets.ppy.sh/beatmaps/39804/covers/cover.jpg?1456506845",
                                    },
                                },
                            },
                        },
                    },
                    MaxParticipants = { Value = 10 },
                    Participants =
                    {
                        Value = new[]
                        {
                            new User { Username = @"Angelsim", Id = 1777162, Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 4 } } },
                            new User { Username = @"HappyStick", Id = 256802, Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 752 } } },
                            new User { Username = @"-Konpaku-", Id = 2258797, Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 571 } } }
                        }
                    }
                },
            });

            AddStep(@"clear rooms", () => lobby.Rooms = new Room[] { });
        }
    }
}
