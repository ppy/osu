// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("create list", () =>
            {
                SelectedRoom.Value = new Room { RoomID = { Value = 7 } };

                for (int i = 0; i < 50; i++)
                {
                    SelectedRoom.Value.RecentParticipants.Add(new APIUser
                    {
                        Username = "peppy",
                        Statistics = new UserStatistics { GlobalRank = 1234 },
                        Id = 2
                    });
                }
            });
        }

        [Test]
        public void TestHorizontalLayout()
        {
            AddStep("create component", () =>
            {
                Child = new ParticipantsDisplay(Direction.Horizontal)
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
                Child = new ParticipantsDisplay(Direction.Vertical)
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
