// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Testing;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Tests.Visual.OnlinePlay;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneRoomListing : OnlinePlayTestScene
    {
        private BindableList<Room> rooms = null!;
        private IBindable<Room?> selectedRoom = null!;
        private RoomListing container = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("create container", () =>
            {
                rooms = new BindableList<Room>();
                selectedRoom = new Bindable<Room?>();

                Child = new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 0.5f,
                    Child = container = new RoomListing
                    {
                        RelativeSizeAxes = Axes.Both,
                        Rooms = { BindTarget = rooms },
                        SelectedRoom = { BindTarget = selectedRoom }
                    }
                };
            });
        }

        [Test]
        public void TestBasicListChanges()
        {
            AddStep("add rooms", () => rooms.AddRange(GenerateRooms(5, withPinnedRooms: true)));

            AddAssert("has 5 rooms", () => container.DrawableRooms.Count == 5);

            AddAssert("all pinned at top", () => container.DrawableRooms
                                                          .SkipWhile(r => r.Room.Pinned)
                                                          .All(r => !r.Room.Pinned));

            AddStep("remove first room", () => rooms.RemoveAt(0));
            AddAssert("has 4 rooms", () => container.DrawableRooms.Count == 4);
            AddAssert("first room removed", () => container.DrawableRooms.All(r => r.Room.RoomID != 0));

            AddStep("select first room", () => container.DrawableRooms.First().TriggerClick());
            AddAssert("first pinned room selected", () => checkRoomSelected(rooms.First(r => r.Pinned)));

            AddStep("remove last room", () => rooms.RemoveAt(rooms.Count - 1));
            AddAssert("first pinned room selected", () => checkRoomSelected(rooms.First(r => r.Pinned)));

            AddStep("remove pinned rooms", () => rooms.RemoveAll(r => r.Pinned));
            AddAssert("selection vacated", () => checkRoomSelected(null));
        }

        [Test]
        public void TestKeyboardNavigation()
        {
            AddStep("add rooms", () => rooms.AddRange(GenerateRooms(3)));

            AddAssert("no selection", () => checkRoomSelected(null));

            press(Key.Down);
            AddAssert("first room selected", () => checkRoomSelected(container.DrawableRooms.First().Room));

            press(Key.Up);
            AddAssert("first room selected", () => checkRoomSelected(container.DrawableRooms.First().Room));

            press(Key.Down);
            press(Key.Down);
            AddAssert("last room selected", () => checkRoomSelected(container.DrawableRooms.Last().Room));
        }

        [Test]
        public void TestKeyboardNavigationAfterOrderChange()
        {
            AddStep("add rooms", () => rooms.AddRange(GenerateRooms(3)));

            AddStep("reorder rooms", () =>
            {
                var room = rooms[1];
                rooms.Remove(room);

                room.RoomID += 3;
                rooms.Add(room);
            });

            AddAssert("no selection", () => checkRoomSelected(null));

            press(Key.Down);
            AddAssert("first room selected", () => checkRoomSelected(getRoomInFlow(0)));

            press(Key.Down);
            AddAssert("second room selected", () => checkRoomSelected(getRoomInFlow(1)));

            press(Key.Down);
            AddAssert("third room selected", () => checkRoomSelected(getRoomInFlow(2)));
        }

        [Test]
        public void TestClickDeselection()
        {
            AddStep("add room", () => rooms.AddRange(GenerateRooms(1)));

            AddAssert("no selection", () => checkRoomSelected(null));

            press(Key.Down);
            AddAssert("first room selected", () => checkRoomSelected(container.DrawableRooms.First().Room));

            AddStep("click away", () => InputManager.Click(MouseButton.Left));
            AddAssert("no selection", () => checkRoomSelected(null));
        }

        private void press(Key down)
        {
            AddStep($"press {down}", () => InputManager.Key(down));
        }

        [Test]
        public void TestStringFiltering()
        {
            AddStep("add rooms", () => rooms.AddRange(GenerateRooms(4)));

            AddUntilStep("4 rooms visible", () => container.DrawableRooms.Count(r => r.IsPresent) == 4);

            AddStep("filter one room", () => container.Filter.Value = new FilterCriteria { SearchString = rooms.First().Name });

            AddUntilStep("1 rooms visible", () => container.DrawableRooms.Count(r => r.IsPresent) == 1);

            AddStep("remove filter", () => container.Filter.Value = null);

            AddUntilStep("4 rooms visible", () => container.DrawableRooms.Count(r => r.IsPresent) == 4);
        }

        [Test]
        public void TestRulesetFiltering()
        {
            AddStep("add rooms", () => rooms.AddRange(GenerateRooms(2, new OsuRuleset().RulesetInfo)));
            AddStep("add rooms", () => rooms.AddRange(GenerateRooms(3, new CatchRuleset().RulesetInfo)));

            // Todo: What even is this case...?
            AddStep("set empty filter criteria", () => container.Filter.Value = new FilterCriteria());
            AddUntilStep("5 rooms visible", () => container.DrawableRooms.Count(r => r.IsPresent) == 5);

            AddStep("filter osu! rooms", () => container.Filter.Value = new FilterCriteria { Ruleset = new OsuRuleset().RulesetInfo });
            AddUntilStep("2 rooms visible", () => container.DrawableRooms.Count(r => r.IsPresent) == 2);

            AddStep("filter catch rooms", () => container.Filter.Value = new FilterCriteria { Ruleset = new CatchRuleset().RulesetInfo });
            AddUntilStep("3 rooms visible", () => container.DrawableRooms.Count(r => r.IsPresent) == 3);
        }

        [Test]
        public void TestAccessTypeFiltering()
        {
            AddStep("add rooms", () =>
            {
                rooms.AddRange(GenerateRooms(1, withPassword: true));
                rooms.AddRange(GenerateRooms(1, withPassword: false));
            });

            AddStep("apply default filter", () => container.Filter.SetDefault());

            AddUntilStep("both rooms visible", () => container.DrawableRooms.Count(r => r.IsPresent) == 2);

            AddStep("filter public rooms", () => container.Filter.Value = new FilterCriteria { Permissions = RoomPermissionsFilter.Public });

            AddUntilStep("private room hidden", () => container.DrawableRooms.All(r => !r.Room.HasPassword));

            AddStep("filter private rooms", () => container.Filter.Value = new FilterCriteria { Permissions = RoomPermissionsFilter.Private });

            AddUntilStep("public room hidden", () => container.DrawableRooms.All(r => r.Room.HasPassword));
        }

        [Test]
        public void TestPasswordProtectedRooms()
        {
            AddStep("add rooms", () => rooms.AddRange(GenerateRooms(3, withPassword: true)));
        }

        private bool checkRoomSelected(Room? room) => selectedRoom.Value == room;

        private Room? getRoomInFlow(int index) =>
            (container.ChildrenOfType<FillFlowContainer<LoungeRoomPanel>>().First().FlowingChildren.ElementAt(index) as RoomPanel)?.Room;
    }
}
