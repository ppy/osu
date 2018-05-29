// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Screens.Multi.Screens.Match;
using osu.Game.Users;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
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
                MaxParticipants = { Value = 5 },
                Participants =
                {
                    Value = new[]
                    {
                        new User
                        {
                            Username = @"eiri-",
                            Id = 3388410,
                            Country = new Country { FlagName = @"US" },
                            CoverUrl = @"https://assets.ppy.sh/user-profile-covers/3388410/00a8486a247831e1cc4375db519f611ac970bda8bc0057d78b0f540ea38c3e58.jpeg",
                            IsSupporter = true,
                        },
                        new User
                        {
                            Username = @"Nepuri",
                            Id = 6637817,
                            Country = new Country { FlagName = @"DE" },
                            CoverUrl = @"https://assets.ppy.sh/user-profile-covers/6637817/9085fc60248b6b5327a72c1dcdecf2dbedba810ae0ab6bcf7224e46b1339632a.jpeg",
                            IsSupporter = true,
                        },
                        new User
                        {
                            Username = @"goheegy",
                            Id = 8057655,
                            Country = new Country { FlagName = @"GB" },
                            CoverUrl = @"https://assets.ppy.sh/user-profile-covers/8057655/21cec27c25a11dc197a4ec6a74253dbabb495949b0e0697113352f12007018c5.jpeg",
                        },
                        new User
                        {
                            Username = @"Alumetri",
                            Id = 5371497,
                            Country = new Country { FlagName = @"RU" },
                            CoverUrl = @"https://assets.ppy.sh/user-profile-covers/5371497/e023b8c7fbe3613e64bd4856703517ea50fbed8a5805dc9acda9efe9897c67e2.jpeg",
                        },
                    }
                },
            };

            Match match = new Match(room);

            AddStep(@"show", () => Add(match));
            AddStep(@"null beatmap", () => room.Beatmap.Value = null);
            AddStep(@"change name", () => room.Name.Value = @"Two Awesome Rooms");
            AddStep(@"change status", () => room.Status.Value = new RoomStatusPlaying());
            AddStep(@"change availability", () => room.Availability.Value = RoomAvailability.FriendsOnly);
            AddStep(@"change type", () => room.Type.Value = new GameTypeTag());
            AddStep(@"change beatmap", () => room.Beatmap.Value = new BeatmapInfo
            {
                StarDifficulty = 4.33,
                Ruleset = rulesets.GetRuleset(2),
                Metadata = new BeatmapMetadata
                {
                    Title = @"Yasashisa no Riyuu",
                    Artist = @"ChouCho",
                    AuthorString = @"celerih",
                },
                BeatmapSet = new BeatmapSetInfo
                {
                    OnlineInfo = new BeatmapSetOnlineInfo
                    {
                        Covers = new BeatmapSetOnlineCovers
                        {
                            Cover = @"https://assets.ppy.sh/beatmaps/685391/covers/cover.jpg?1524597970",
                        },
                    },
                },
            });

            AddStep(@"null max participants", () => room.MaxParticipants.Value = null);
            AddStep(@"change participants", () => room.Participants.Value = new[]
            {
                new User
                {
                    Username = @"Spectator",
                    Id = 702598,
                    Country = new Country { FlagName = @"KR" },
                    CoverUrl = @"https://assets.ppy.sh/user-profile-covers/702598/3bbf4cb8b8d2cf8b03145000a975ff27e191ab99b0920832e7dd67386280e288.jpeg",
                    IsSupporter = true,
                },
                new User
                {
                    Username = @"celerih",
                    Id = 4696296,
                    Country = new Country { FlagName = @"CA" },
                    CoverUrl = @"https://assets.ppy.sh/user-profile-covers/4696296/7f8500731d0ac66d5472569d146a7be07d9460273361913f22c038867baddaef.jpeg",
                },
            });

            AddStep(@"exit", match.Exit);
        }
    }
}
