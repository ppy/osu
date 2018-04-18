// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Screens.Multiplayer;
using osu.Game.Users;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseDrawableRoom : OsuTestCase
    {
        private RulesetStore rulesets;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            DrawableRoom first;
            Add(new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                Width = 580f,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    first = new DrawableRoom(new Room
                    {
                        Name = { Value = @"Great Room Right Here" },
                        Host = { Value = new User { Username = @"Naeferith", Id = 9492835, Country = new Country { FlagName = @"FR" } } },
                        Status = { Value = new RoomStatusOpen() },
                        Type = { Value = new GameTypeTeamVersus() },
                        Beatmap =
                        {
                            Value = new BeatmapInfo
                            {
                                StarDifficulty = 4.65,
                                Ruleset = rulesets.GetRuleset(3),
                                Metadata = new BeatmapMetadata
                                {
                                    Title = @"Critical Crystal",
                                    Artist = @"Seiryu",
                                },
                                BeatmapSet = new BeatmapSetInfo
                                {
                                    OnlineInfo = new BeatmapSetOnlineInfo
                                    {
                                        Covers = new BeatmapSetOnlineCovers
                                        {
                                            Cover = @"https://assets.ppy.sh//beatmaps/376340/covers/cover.jpg?1456478455",
                                        },
                                    },
                                },
                            },
                        },
                        Participants =
                        {
                            Value = new[]
                            {
                                new User { Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 1355 } } },
                                new User { Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 8756 } } },
                            },
                        },
                    }),
                    new DrawableRoom(new Room
                    {
                        Name = { Value = @"Relax It's The Weekend" },
                        Host = { Value = new User { Username = @"peppy", Id = 2, Country = new Country { FlagName = @"AU" } } },
                        Status = { Value = new RoomStatusPlaying() },
                        Type = { Value = new GameTypeTagTeam() },
                        Beatmap =
                        {
                            Value = new BeatmapInfo
                            {
                                StarDifficulty = 1.96,
                                Ruleset = rulesets.GetRuleset(0),
                                Metadata = new BeatmapMetadata
                                {
                                    Title = @"Serendipity",
                                    Artist = @"ZAQ",
                                },
                                BeatmapSet = new BeatmapSetInfo
                                {
                                    OnlineInfo = new BeatmapSetOnlineInfo
                                    {
                                        Covers = new BeatmapSetOnlineCovers
                                        {
                                            Cover = @"https://assets.ppy.sh//beatmaps/526839/covers/cover.jpg?1493815706",
                                        },
                                    },
                                },
                            },
                        },
                        Participants =
                        {
                            Value = new[]
                            {
                                new User { Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 578975 } } },
                                new User { Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 24554 } } },
                            },
                        },
                    }),
                }
            });

            AddStep(@"change title", () => first.Room.Name.Value = @"I Changed Name");
            AddStep(@"change host", () => first.Room.Host.Value = new User { Username = @"DrabWeb", Id = 6946022, Country = new Country { FlagName = @"CA" } });
            AddStep(@"change status", () => first.Room.Status.Value = new RoomStatusPlaying());
            AddStep(@"change type", () => first.Room.Type.Value = new GameTypeVersus());
            AddStep(@"change beatmap", () => first.Room.Beatmap.Value = null);
            AddStep(@"change participants", () => first.Room.Participants.Value = new[]
            {
                new User { Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 1254 } } },
                new User { Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 123189 } } },
            });
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            this.rulesets = rulesets;
        }
    }
}
