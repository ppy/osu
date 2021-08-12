// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Tests.Visual.OnlinePlay;
using osu.Game.Users;
using osu.Game.Users.Drawables;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneRecentParticipantsList : OnlinePlayTestScene
    {
        private RecentParticipantsList list;

        [Cached]
        protected readonly OverlayColourProvider ColourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            SelectedRoom.Value = new Room { Name = { Value = "test room" } };

            Child = list = new RecentParticipantsList
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                NumberOfCircles = 3
            };
        });

        [Test]
        public void TestCircleCountNearLimit()
        {
            AddStep("add 8 users", () =>
            {
                for (int i = 0; i < 8; i++)
                    addUser(i);
            });
            AddStep("set 8 circles", () => list.NumberOfCircles = 8);
            AddAssert("0 hidden users", () => list.ChildrenOfType<RecentParticipantsList.HiddenUserCount>().Single().Count == 0);

            AddStep("add one more user", () => addUser(9));
            AddAssert("2 hidden users", () => list.ChildrenOfType<RecentParticipantsList.HiddenUserCount>().Single().Count == 2);

            AddStep("remove first user", () => removeUserAt(0));
            AddAssert("0 hidden users", () => list.ChildrenOfType<RecentParticipantsList.HiddenUserCount>().Single().Count == 0);

            AddStep("add one more user", () => addUser(9));
            AddAssert("2 hidden users", () => list.ChildrenOfType<RecentParticipantsList.HiddenUserCount>().Single().Count == 2);

            AddStep("remove last user", () => removeUserAt(8));
            AddAssert("0 hidden users", () => list.ChildrenOfType<RecentParticipantsList.HiddenUserCount>().Single().Count == 0);
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
            AddAssert("3 circles displayed", () => list.ChildrenOfType<UpdateableAvatar>().Count() == 3);
            AddAssert("47 hidden users", () => list.ChildrenOfType<RecentParticipantsList.HiddenUserCount>().Single().Count == 47);

            AddStep("set 10 circles", () => list.NumberOfCircles = 10);
            AddAssert("10 circles displayed", () => list.ChildrenOfType<UpdateableAvatar>().Count() == 10);
            AddAssert("40 hidden users", () => list.ChildrenOfType<RecentParticipantsList.HiddenUserCount>().Single().Count == 40);
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
            AddAssert("3 circles displayed", () => list.ChildrenOfType<UpdateableAvatar>().Count() == 3);
            AddAssert("46 hidden users", () => list.ChildrenOfType<RecentParticipantsList.HiddenUserCount>().Single().Count == 46);

            AddStep("remove from end", () => removeUserAt(SelectedRoom.Value.RecentParticipants.Count - 1));
            AddAssert("3 circles displayed", () => list.ChildrenOfType<UpdateableAvatar>().Count() == 3);
            AddAssert("45 hidden users", () => list.ChildrenOfType<RecentParticipantsList.HiddenUserCount>().Single().Count == 45);

            AddRepeatStep("remove 45 users", () => removeUserAt(0), 45);
            AddAssert("3 circles displayed", () => list.ChildrenOfType<UpdateableAvatar>().Count() == 3);
            AddAssert("0 hidden users", () => list.ChildrenOfType<RecentParticipantsList.HiddenUserCount>().Single().Count == 0);
            AddAssert("hidden users bubble hidden", () => list.ChildrenOfType<RecentParticipantsList.HiddenUserCount>().Single().Alpha < 0.5f);

            AddStep("remove another user", () => removeUserAt(0));
            AddAssert("2 circles displayed", () => list.ChildrenOfType<UpdateableAvatar>().Count() == 2);
            AddAssert("0 hidden users", () => list.ChildrenOfType<RecentParticipantsList.HiddenUserCount>().Single().Count == 0);

            AddRepeatStep("remove the remaining two users", () => removeUserAt(0), 2);
            AddAssert("0 circles displayed", () => !list.ChildrenOfType<UpdateableAvatar>().Any());
        }

        private void addUser(int id)
        {
            SelectedRoom.Value.ParticipantCount.Value++;
            SelectedRoom.Value.RecentParticipants.Add(new User
            {
                Id = id,
                Username = $"User {id}"
            });
        }

        private void removeUserAt(int index)
        {
            SelectedRoom.Value.ParticipantCount.Value--;
            SelectedRoom.Value.RecentParticipants.RemoveAt(index);
        }
    }
}
