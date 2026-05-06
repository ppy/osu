// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Overlays.Profile;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osuTK.Input;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneUserReport : RankedPlayTestScene
    {
        private RankedPlayScreen screen = null!;
        private UserReportDialog dialog = null!;

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            UserReportRequest pendingRequest = null!;

            AddStep("setup request handling", () =>
            {
                dummyAPI.HandleRequest += request =>
                {
                    if (request is UserReportRequest chatReportRequest)
                    {
                        pendingRequest = chatReportRequest;
                        return true;
                    }

                    return false;
                };
            });

            AddStep("join room", () => JoinRoom(CreateDefaultRoom(MatchType.RankedPlay)));
            WaitForJoined();

            AddStep("add other user", () => MultiplayerClient.AddUser(new MultiplayerRoomUser(2)));

            AddStep("load screen", () => LoadScreen(screen = new RankedPlayScreen(MultiplayerClient.ClientRoom!)));
            AddUntilStep("screen loaded", () => screen.IsLoaded);
            AddStep("set pick state", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardPlay, state => state.ActiveUserId = API.LocalUser.Value.OnlineID).WaitSafely());

            AddStep("open hamburger menu", () => this.ChildrenOfType<HamburgerMenu>().Single().TriggerClick());
            AddStep("select report option", () =>
            {
                var text = this.ChildrenOfType<OsuPopover>().Single().ChildrenOfType<OsuSpriteText>().Single(t => t.Text == "Report opponent");
                InputManager.MoveMouseTo(text);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("report dialog is present", () => (dialog = this.ChildrenOfType<UserReportDialog>().Single()).IsPresent, () => Is.True);

            AddStep("input reason", () => dialog.ChildrenOfType<OsuTextBox>().First().Text = "reason");
            AddStep("send report", () => dialog.ChildrenOfType<Button>().First().TriggerClick());

            AddWaitStep("wait", 5);
            AddStep("complete request", () => pendingRequest.TriggerSuccess());
            AddUntilStep("wait for dialog to hide", () => this.ChildrenOfType<UserReportDialog>().Any(), () => Is.False);
        }
    }
}
