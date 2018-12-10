// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Multi.Lounge;
using osu.Game.Screens.Multi.Lounge.Components;
using osu.Game.Users;
using osuTK.Input;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseLounge : ManualInputManagerTestCase
    {
        private TestLoungeScreen loungeScreen;

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            loungeScreen = new TestLoungeScreen();

            Room[] rooms =
            {
                new Room
                {
                    Name = { Value = @"Just Another Room" },
                    Host = { Value = new User { Username = @"DrabWeb", Id = 6946022, Country = new Country { FlagName = @"CA" } } },
                    Status = { Value = new RoomStatusPlaying() },
                    Availability = { Value = RoomAvailability.Public },
                    Type = { Value = new GameTypeTagTeam() },
                    Beatmap =
                    {
                        Value = new BeatmapInfo
                        {
                            StarDifficulty = 5.65,
                            Ruleset = rulesets.GetRuleset(0),
                            Metadata = new BeatmapMetadata
                            {
                                Title = @"Sidetracked Day (Short Ver.)",
                                Artist = @"VINXIS",
                                AuthorString = @"Hobbes2",
                            },
                            BeatmapSet = new BeatmapSetInfo
                            {
                                OnlineInfo = new BeatmapSetOnlineInfo
                                {
                                    Covers = new BeatmapSetOnlineCovers
                                    {
                                        Cover = @"https://assets.ppy.sh/beatmaps/767600/covers/cover.jpg?1526243446",
                                    },
                                },
                            },
                        }
                    },
                    MaxParticipants = { Value = 10 },
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
                        }
                    }
                },
                new Room
                {
                    Name = { Value = @"Not Just Any Room" },
                    Host = { Value = new User { Username = @"Monstrata", Id = 2706438, Country = new Country { FlagName = @"CA" } } },
                    Status = { Value = new RoomStatusOpen() },
                    Availability = { Value = RoomAvailability.FriendsOnly },
                    Type = { Value = new GameTypeTeamVersus() },
                    Beatmap =
                    {
                        Value = new BeatmapInfo
                        {
                            StarDifficulty = 2.73,
                            Ruleset = rulesets.GetRuleset(0),
                            Metadata = new BeatmapMetadata
                            {
                                Title = @"lit(var)",
                                Artist = @"kensuke ushio",
                                AuthorString = @"Monstrata",
                            },
                            BeatmapSet = new BeatmapSetInfo
                            {
                                OnlineInfo = new BeatmapSetOnlineInfo
                                {
                                    Covers = new BeatmapSetOnlineCovers
                                    {
                                        Cover = @"https://assets.ppy.sh/beatmaps/623972/covers/cover.jpg?1521167183",
                                    },
                                },
                            },
                        }
                    },
                    Participants =
                    {
                        Value = new[]
                        {
                            new User { Username = @"Jeby", Id = 3136279, Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 3497 } } },
                            new User { Username = @"DualAkira", Id = 5220933, Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 643 } } },
                            new User { Username = @"Datenshi Yohane", Id = 7171857, Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 10555 } } },
                        }
                    }
                },
                new Room
                {
                    Name = { Value = @"room THE FINAL" },
                    Host = { Value = new User { Username = @"Delis", Id = 1603923, Country = new Country { FlagName = @"JP" } } },
                    Status = { Value = new RoomStatusPlaying() },
                    Availability = { Value = RoomAvailability.Public },
                    Type = { Value = new GameTypeTagTeam() },
                    Beatmap =
                    {
                        Value = new BeatmapInfo
                        {
                            StarDifficulty = 4.48,
                            Ruleset = rulesets.GetRuleset(3),
                            Metadata = new BeatmapMetadata
                            {
                                Title = @"ONIGIRI FREEWAY",
                                Artist = @"OISHII",
                                AuthorString = @"Mentholzzz",
                            },
                            BeatmapSet = new BeatmapSetInfo
                            {
                                OnlineInfo = new BeatmapSetOnlineInfo
                                {
                                    Covers = new BeatmapSetOnlineCovers
                                    {
                                        Cover = @"https://assets.ppy.sh/beatmaps/663098/covers/cover.jpg?1521898837",
                                    },
                                },
                            },
                        }
                    },
                    MaxParticipants = { Value = 30 },
                    Participants =
                    {
                        Value = new[]
                        {
                            new User { Username = @"KizuA", Id = 6510442, Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 5372 } } },
                            new User { Username = @"Colored", Id = 827563, Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 810 } } },
                            new User { Username = @"Beryl", Id = 3817591, Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 10096 } } },
                        }
                    }
                },
            };

            AddStep(@"show", () => Add(loungeScreen));
            AddStep(@"set rooms", () => loungeScreen.Rooms = rooms);
            selectAssert(0);
            AddStep(@"clear rooms", () => loungeScreen.Rooms = new Room[] {});
            AddAssert(@"no room selected", () => loungeScreen.SelectedRoom == null);
            AddStep(@"set rooms", () => loungeScreen.Rooms = rooms);
            selectAssert(1);
            AddStep(@"open room 1", () => clickRoom(1));
            AddUntilStep(() => loungeScreen.ChildScreen?.IsCurrentScreen == true, "wait until room current");
            AddStep(@"make lounge current", loungeScreen.MakeCurrent);
            filterAssert(@"THE FINAL", LoungeTab.Public, 1);
            filterAssert(string.Empty, LoungeTab.Public, 2);
            filterAssert(string.Empty, LoungeTab.Private, 1);
            filterAssert(string.Empty, LoungeTab.Public, 2);
            filterAssert(@"no matches", LoungeTab.Public, 0);
            AddStep(@"clear rooms", () => loungeScreen.Rooms = new Room[] {});
            AddStep(@"set rooms", () => loungeScreen.Rooms = rooms);
            AddAssert(@"no matches after clear", () => !loungeScreen.ChildRooms.Any());
            filterAssert(string.Empty, LoungeTab.Public, 2);
            AddStep(@"exit", loungeScreen.Exit);
        }

        private void clickRoom(int n)
        {
            InputManager.MoveMouseTo(loungeScreen.ChildRooms.ElementAt(n));
            InputManager.Click(MouseButton.Left);
        }

        private void selectAssert(int n)
        {
            AddStep($@"select room {n}", () => clickRoom(n));
            AddAssert($@"room {n} selected", () => loungeScreen.SelectedRoom == loungeScreen.ChildRooms.ElementAt(n).Room);
        }

        private void filterAssert(string filter, LoungeTab tab, int endCount)
        {
            AddStep($@"filter '{filter}', {tab}", () => loungeScreen.SetFilter(filter, tab));
            AddAssert(@"filtered correctly", () => loungeScreen.ChildRooms.Count() == endCount);
        }

        private class TestLoungeScreen : LoungeScreen
        {
            protected override BackgroundScreen CreateBackground() => new BackgroundScreenDefault();

            public IEnumerable<DrawableRoom> ChildRooms => RoomsContainer.Children.Where(r => r.MatchingFilter);
            public Room SelectedRoom => Inspector.Room;

            public void SetFilter(string filter, LoungeTab tab)
            {
                Filter.Search.Current.Value = filter;
                Filter.Tabs.Current.Value = tab;
            }
        }
    }
}
