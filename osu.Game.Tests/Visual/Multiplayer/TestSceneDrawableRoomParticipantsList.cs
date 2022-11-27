// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Tests.Visual.OnlinePlay;
using osu.Game.Users.Drawables;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneDrawableRoomParticipantsList : OnlinePlayTestScene
    {
        private DrawableRoomParticipantsList list;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("create list", () =>
            {
                SelectedRoom.Value = new Room
                {
                    Name = { Value = "test room" },
                    Host =
                    {
                        Value = new APIUser
                        {
                            Id = 2,
                            Username = "peppy",
                        }
                    }
                };

                Child = list = new DrawableRoomParticipantsList
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    NumberOfCircles = 4
                };
            });
        }

        [Test]
        public void TestCircleCountNearLimit()
        {
            AddStep("add 8 users", () =>
            {
                for (int i = 0; i < 8; i++)
                    addUser(i);
            });

            AddStep("set 8 circles", () => list.NumberOfCircles = 8);
            AddAssert("0 hidden users", () => list.ChildrenOfType<DrawableRoomParticipantsList.HiddenUserCount>().Single().Count == 0);

            AddStep("add one more user", () => addUser(9));
            AddAssert("2 hidden users", () => list.ChildrenOfType<DrawableRoomParticipantsList.HiddenUserCount>().Single().Count == 2);

            AddStep("remove first user", () => removeUserAt(0));
            AddAssert("0 hidden users", () => list.ChildrenOfType<DrawableRoomParticipantsList.HiddenUserCount>().Single().Count == 0);

            AddStep("add one more user", () => addUser(9));
            AddAssert("2 hidden users", () => list.ChildrenOfType<DrawableRoomParticipantsList.HiddenUserCount>().Single().Count == 2);

            AddStep("remove last user", () => removeUserAt(8));
            AddAssert("0 hidden users", () => list.ChildrenOfType<DrawableRoomParticipantsList.HiddenUserCount>().Single().Count == 0);
        }

        [Test]
        public void TestHiddenUsersBecomeDisplayed()
        {
            AddStep("add 8 users", () =>
            {
                for (int i = 0; i < 8; i++)
                    addUser(i);
            });

            AddStep("set 3 circles", () => list.NumberOfCircles = 3);

            for (int i = 0; i < 8; i++)
            {
                AddStep("remove user", () => removeUserAt(0));
                int remainingUsers = 8 - i;

                int displayedUsers = remainingUsers > 4 ? 3 : remainingUsers;
                AddAssert($"{displayedUsers} avatars displayed", () => list.ChildrenOfType<UpdateableAvatar>().Count() == displayedUsers);
            }
        }

        [Test]
        public void TestCircleCount()
        {
            AddStep("add 50 users", () =>
            {
                for (int i = 0; i < 50; i++)
                    addUser(i);
            });

            AddStep("set 3 circles", () => list.NumberOfCircles = 3);
            AddAssert("3 users displayed", () => list.ChildrenOfType<UpdateableAvatar>().Count() == 3);
            AddAssert("48 hidden users", () => list.ChildrenOfType<DrawableRoomParticipantsList.HiddenUserCount>().Single().Count == 48);

            AddStep("set 10 circles", () => list.NumberOfCircles = 10);
            AddAssert("10 users displayed", () => list.ChildrenOfType<UpdateableAvatar>().Count() == 10);
            AddAssert("41 hidden users", () => list.ChildrenOfType<DrawableRoomParticipantsList.HiddenUserCount>().Single().Count == 41);
        }

        [Test]
        public void TestAddAndRemoveUsers()
        {
            AddStep("add 50 users", () =>
            {
                for (int i = 0; i < 50; i++)
                    addUser(i);
            });

            AddStep("remove from start", () => removeUserAt(0));
            AddAssert("4 circles displayed", () => list.ChildrenOfType<UpdateableAvatar>().Count() == 4);
            AddAssert("46 hidden users", () => list.ChildrenOfType<DrawableRoomParticipantsList.HiddenUserCount>().Single().Count == 46);

            AddStep("remove from end", () => removeUserAt(SelectedRoom.Value.RecentParticipants.Count - 1));
            AddAssert("4 circles displayed", () => list.ChildrenOfType<UpdateableAvatar>().Count() == 4);
            AddAssert("45 hidden users", () => list.ChildrenOfType<DrawableRoomParticipantsList.HiddenUserCount>().Single().Count == 45);

            AddRepeatStep("remove 45 users", () => removeUserAt(0), 45);
            AddAssert("4 circles displayed", () => list.ChildrenOfType<UpdateableAvatar>().Count() == 4);
            AddAssert("0 hidden users", () => list.ChildrenOfType<DrawableRoomParticipantsList.HiddenUserCount>().Single().Count == 0);
            AddAssert("hidden users bubble hidden", () => list.ChildrenOfType<DrawableRoomParticipantsList.HiddenUserCount>().Single().Alpha < 0.5f);

            AddStep("remove another user", () => removeUserAt(0));
            AddAssert("3 circles displayed", () => list.ChildrenOfType<UpdateableAvatar>().Count() == 3);
            AddAssert("0 hidden users", () => list.ChildrenOfType<DrawableRoomParticipantsList.HiddenUserCount>().Single().Count == 0);

            AddRepeatStep("remove the remaining two users", () => removeUserAt(0), 2);
            AddAssert("1 circle displayed", () => list.ChildrenOfType<UpdateableAvatar>().Count() == 1);
        }

        private void addUser(int id)
        {
            SelectedRoom.Value.RecentParticipants.Add(new APIUser
            {
                Id = id,
                Username = $"User {id}"
            });
            SelectedRoom.Value.ParticipantCount.Value++;
        }

        private void removeUserAt(int index)
        {
            SelectedRoom.Value.RecentParticipants.RemoveAt(index);
            SelectedRoom.Value.ParticipantCount.Value--;
        }
    }
}
