// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Tests.Visual.OnlinePlay;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Playlists
{
    public class TestScenePlaylistsParticipantsList : OsuTestScene
    {
        private TestRoomContainer roomContainer;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = roomContainer = new TestRoomContainer
            {
                Room = { RoomID = { Value = 7 } }
            };

            for (int i = 0; i < 50; i++)
            {
                roomContainer.Room.RecentParticipants.Add(new User
                {
                    Username = "peppy",
                    Statistics = new UserStatistics { GlobalRank = 1234 },
                    Id = 2
                });
            }
        });

        [Test]
        public void TestHorizontalLayout()
        {
            AddStep("create component", () =>
            {
                roomContainer.Child = new ParticipantsDisplay(Direction.Horizontal)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 0.2f,
                };
            });
        }

        [Test]
        public void TestVerticalLayout()
        {
            AddStep("create component", () =>
            {
                roomContainer.Child = new ParticipantsDisplay(Direction.Vertical)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 0.2f,
                    Height = 0.2f,
                };
            });
        }
    }
}
