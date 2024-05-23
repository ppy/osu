// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.Metadata;
using osu.Game.Online.Rooms;
using osu.Game.Screens.Menu;
using osuTK.Input;
using Color4 = osuTK.Graphics.Color4;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneMainMenuButton : OsuTestScene
    {
        [Resolved]
        private MetadataClient metadataClient { get; set; } = null!;

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        [Test]
        public void TestStandardButton()
        {
            AddStep("add button", () => Child = new MainMenuButton(
                ButtonSystemStrings.Solo, @"button-default-select", OsuIcon.Player, new Color4(102, 68, 204, 255), _ => { }, 0, Key.P)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                ButtonSystemState = ButtonSystemState.TopLevel,
            });
        }

        [Test]
        public void TestDailyChallengeButton()
        {
            AddStep("beatmap of the day not active", () => metadataClient.DailyChallengeUpdated(null));

            AddStep("set up API", () => dummyAPI.HandleRequest = req =>
            {
                switch (req)
                {
                    case GetRoomRequest getRoomRequest:
                        if (getRoomRequest.RoomId != 1234)
                            return false;

                        var beatmap = CreateAPIBeatmap();
                        beatmap.OnlineID = 1001;
                        getRoomRequest.TriggerSuccess(new Room
                        {
                            RoomID = { Value = 1234 },
                            Playlist =
                            {
                                new PlaylistItem(beatmap)
                            },
                            EndDate = { Value = DateTimeOffset.Now.AddSeconds(30) }
                        });
                        return true;

                    default:
                        return false;
                }
            });

            AddStep("add button", () => Child = new DailyChallengeButton(@"button-default-select", new Color4(102, 68, 204, 255), _ => { }, 0, Key.D)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                ButtonSystemState = ButtonSystemState.TopLevel,
            });

            AddStep("beatmap of the day active", () => metadataClient.DailyChallengeUpdated(new DailyChallengeInfo
            {
                RoomID = 1234,
            }));
        }
    }
}
