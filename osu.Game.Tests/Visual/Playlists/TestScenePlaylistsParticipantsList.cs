// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Tests.Visual.OnlinePlay;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Playlists
{
    public partial class TestScenePlaylistsParticipantsList : OnlinePlayTestScene
    {
        private Room room = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("create room", () =>
            {
                room = new Room
                {
                    RoomID = 7,
                    RecentParticipants = Enumerable.Range(0, 50).Select(_ => new APIUser
                    {
                        Username = "peppy",
                        Statistics = new UserStatistics { GlobalRank = 1234 },
                        Id = 2
                    }).ToArray()
                };
            });
        }

        [Test]
        public void TestHorizontalLayout()
        {
            AddStep("create component", () =>
            {
                Child = new ParticipantsDisplay(room, Direction.Horizontal)
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
                Child = new ParticipantsDisplay(room, Direction.Vertical)
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
