// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Multi;
using osu.Game.Screens.Multi.Lounge.Components;
using osu.Game.Users;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneLoungeRoomsContainer : MultiplayerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(RoomsContainer),
            typeof(DrawableRoom)
        };

        [Cached(Type = typeof(IRoomManager))]
        private TestRoomManager roomManager = new TestRoomManager();

        private RoomsContainer container;

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = container = new RoomsContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 0.5f,
                JoinRequested = joinRequested
            };
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("clear rooms", () => roomManager.Rooms.Clear());
        }

        [Test]
        public void TestBasicListChanges()
        {
            addRooms(3);

            AddAssert("has 3 rooms", () => container.Rooms.Count == 3);
            AddStep("remove first room", () => roomManager.Rooms.Remove(roomManager.Rooms.FirstOrDefault()));
            AddAssert("has 2 rooms", () => container.Rooms.Count == 2);
            AddAssert("first room removed", () => container.Rooms.All(r => r.Room.RoomID.Value != 0));

            AddStep("select first room", () => container.Rooms.First().Action?.Invoke());
            AddAssert("first room selected", () => Room == roomManager.Rooms.First());

            AddStep("join first room", () => container.Rooms.First().Action?.Invoke());
            AddAssert("first room joined", () => roomManager.Rooms.First().Status.Value is JoinedRoomStatus);
        }

        [Test]
        public void TestStringFiltering()
        {
            addRooms(4);

            AddUntilStep("4 rooms visible", () => container.Rooms.Count(r => r.IsPresent) == 4);

            AddStep("filter one room", () => container.Filter(new FilterCriteria { SearchString = "1" }));

            AddUntilStep("1 rooms visible", () => container.Rooms.Count(r => r.IsPresent) == 1);

            AddStep("remove filter", () => container.Filter(null));

            AddUntilStep("4 rooms visible", () => container.Rooms.Count(r => r.IsPresent) == 4);
        }

        [Test]
        public void TestRulesetFiltering()
        {
            addRooms(2, new OsuRuleset().RulesetInfo);
            addRooms(3, new CatchRuleset().RulesetInfo);

            AddUntilStep("5 rooms visible", () => container.Rooms.Count(r => r.IsPresent) == 5);

            AddStep("filter osu! rooms", () => container.Filter(new FilterCriteria { Ruleset = new OsuRuleset().RulesetInfo }));

            AddUntilStep("2 rooms visible", () => container.Rooms.Count(r => r.IsPresent) == 2);

            AddStep("filter catch rooms", () => container.Filter(new FilterCriteria { Ruleset = new CatchRuleset().RulesetInfo }));

            AddUntilStep("3 rooms visible", () => container.Rooms.Count(r => r.IsPresent) == 3);
        }

        private void addRooms(int count, RulesetInfo ruleset = null)
        {
            AddStep("add rooms", () =>
            {
                for (int i = 0; i < count; i++)
                {
                    var room = new Room
                    {
                        RoomID = { Value = i },
                        Name = { Value = $"Room {i}" },
                        Host = { Value = new User { Username = "Host" } },
                        EndDate = { Value = DateTimeOffset.Now + TimeSpan.FromSeconds(10) }
                    };

                    if (ruleset != null)
                    {
                        room.Playlist.Add(new PlaylistItem
                        {
                            Ruleset = { Value = ruleset },
                            Beatmap =
                            {
                                Value = new BeatmapInfo
                                {
                                    Metadata = new BeatmapMetadata()
                                }
                            }
                        });
                    }

                    roomManager.Rooms.Add(room);
                }
            });
        }

        private void joinRequested(Room room) => room.Status.Value = new JoinedRoomStatus();

        private class TestRoomManager : IRoomManager
        {
            public event Action RoomsUpdated
            {
                add { }
                remove { }
            }

            public readonly BindableList<Room> Rooms = new BindableList<Room>();
            IBindableList<Room> IRoomManager.Rooms => Rooms;

            public void CreateRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null) => Rooms.Add(room);

            public void JoinRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
            {
            }

            public void PartRoom()
            {
            }
        }

        private class JoinedRoomStatus : RoomStatus
        {
            public override string Message => "Joined";

            public override Color4 GetAppropriateColour(OsuColour colours) => colours.Yellow;
        }
    }
}
