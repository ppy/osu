// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Tests.Visual.OnlinePlay;
using osu.Game.Users;
using osu.Game.Users.Drawables;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneRecentParticipantsList : OnlinePlayTestScene
    {
        private RecentParticipantsList list;

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            SelectedRoom.Value = new Room { Name = { Value = "test room" } };

            Child = list = new RecentParticipantsList
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                NumberOfAvatars = 3
            };
        });

        [Test]
        public void TestAvatarCount()
        {
            AddStep("add 50 users", () =>
            {
                for (int i = 0; i < 50; i++)
                {
                    SelectedRoom.Value.RecentParticipants.Add(new User
                    {
                        Id = i,
                        Username = $"User {i}"
                    });
                }
            });

            AddStep("set 3 avatars", () => list.NumberOfAvatars = 3);
            AddAssert("3 avatars displayed", () => list.ChildrenOfType<UpdateableAvatar>().Count() == 3);
            AddAssert("47 hidden users", () => list.ChildrenOfType<RecentParticipantsList.HiddenUserCount>().Single().Count == 47);

            AddStep("set 10 avatars", () => list.NumberOfAvatars = 10);
            AddAssert("10 avatars displayed", () => list.ChildrenOfType<UpdateableAvatar>().Count() == 10);
            AddAssert("40 hidden users", () => list.ChildrenOfType<RecentParticipantsList.HiddenUserCount>().Single().Count == 40);
        }

        [Test]
        public void TestAddAndRemoveUsers()
        {
            AddStep("add 50 users", () =>
            {
                for (int i = 0; i < 50; i++)
                {
                    SelectedRoom.Value.RecentParticipants.Add(new User
                    {
                        Id = i,
                        Username = $"User {i}"
                    });
                }
            });

            AddStep("remove from start", () => SelectedRoom.Value.RecentParticipants.RemoveAt(0));
            AddAssert("3 avatars displayed", () => list.ChildrenOfType<UpdateableAvatar>().Count() == 3);
            AddAssert("46 hidden users", () => list.ChildrenOfType<RecentParticipantsList.HiddenUserCount>().Single().Count == 46);

            AddStep("remove from end", () => SelectedRoom.Value.RecentParticipants.RemoveAt(SelectedRoom.Value.RecentParticipants.Count - 1));
            AddAssert("3 avatars displayed", () => list.ChildrenOfType<UpdateableAvatar>().Count() == 3);
            AddAssert("45 hidden users", () => list.ChildrenOfType<RecentParticipantsList.HiddenUserCount>().Single().Count == 45);

            AddRepeatStep("remove 45 users", () => SelectedRoom.Value.RecentParticipants.RemoveAt(0), 45);
            AddAssert("3 avatars displayed", () => list.ChildrenOfType<UpdateableAvatar>().Count() == 3);
            AddAssert("0 hidden users", () => list.ChildrenOfType<RecentParticipantsList.HiddenUserCount>().Single().Count == 0);
            AddAssert("hidden users bubble hidden", () => list.ChildrenOfType<RecentParticipantsList.HiddenUserCount>().Single().Alpha < 0.5f);

            AddStep("remove another user", () => SelectedRoom.Value.RecentParticipants.RemoveAt(0));
            AddAssert("2 avatars displayed", () => list.ChildrenOfType<UpdateableAvatar>().Count() == 2);
            AddAssert("0 hidden users", () => list.ChildrenOfType<RecentParticipantsList.HiddenUserCount>().Single().Count == 0);

            AddRepeatStep("remove the remaining two users", () => SelectedRoom.Value.RecentParticipants.RemoveAt(0), 2);
            AddAssert("0 avatars displayed", () => !list.ChildrenOfType<UpdateableAvatar>().Any());
        }
    }
}
