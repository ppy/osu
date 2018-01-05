// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Screens.Multiplayer;
using osu.Game.Users;

namespace osu.Game.Tests.Visual
{
    public class TestCaseRoomInspector : OsuTestCase
    {
        private RulesetStore rulesets;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var room = new Room
            {
                Name = { Value = @"My Awesome Room" },
                Host = { Value = new User { Username = @"flyte", Id = 3103765, Country = new Country { FlagName = @"JP" } } },
                Status = { Value = new RoomStatusOpen() },
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
                    }
                },
                MaxParticipants = { Value = 200 },
                Participants =
                {
                    Value = new[]
                    {
                        new User { Username = @"flyte", Id = 3103765, GlobalRank = 1425 },
                        new User { Username = @"Cookiezi", Id = 124493, GlobalRank = 5466 },
                        new User { Username = @"Angelsim", Id = 1777162, GlobalRank = 2873 },
                        new User { Username = @"Rafis", Id = 2558286, GlobalRank = 4687 },
                        new User { Username = @"hvick225", Id = 50265, GlobalRank = 3258 },
                        new User { Username = @"peppy", Id = 2, GlobalRank = 6251 }
                    }
                }
            };

            RoomInspector inspector;
            Add(inspector = new RoomInspector
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Room = room,
            });

            AddStep(@"change title", () => room.Name.Value = @"A Better Room Than The Above");
            AddStep(@"change host", () => room.Host.Value = new User { Username = @"DrabWeb", Id = 6946022, Country = new Country { FlagName = @"CA" } });
            AddStep(@"change status", () => room.Status.Value = new RoomStatusPlaying());
            AddStep(@"change type", () => room.Type.Value = new GameTypeTag());
            AddStep(@"change beatmap", () => room.Beatmap.Value = null);
            AddStep(@"change max participants", () => room.MaxParticipants.Value = null);
            AddStep(@"change participants", () => room.Participants.Value = new[]
            {
                new User { Username = @"filsdelama", Id = 2831793, GlobalRank = 8542 },
                new User { Username = @"_index", Id = 652457, GlobalRank = 15024 }
            });

            AddStep(@"change room", () =>
            {
                var newRoom = new Room
                {
                    Name = { Value = @"My New, Better Than Ever Room" },
                    Host = { Value = new User { Username = @"Angelsim", Id = 1777162, Country = new Country { FlagName = @"KR" } } },
                    Status = { Value = new RoomStatusOpen() },
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
                            new User { Username = @"Angelsim", Id = 1777162, GlobalRank = 4 },
                            new User { Username = @"HappyStick", Id = 256802, GlobalRank = 752 },
                            new User { Username = @"-Konpaku-", Id = 2258797, GlobalRank = 571 }
                        }
                    }
                };

                inspector.Room = newRoom;
            });
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            this.rulesets = rulesets;
        }
    }
}
