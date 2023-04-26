// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.OnlinePlay.Multiplayer;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneCreateMultiplayerMatchButton : MultiplayerTestScene
    {
        private CreateMultiplayerMatchButton button;

        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("create button", () => Child = button = new CreateMultiplayerMatchButton
            {
                Width = 200,
                Height = 100,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        }

        [Test]
        public void TestButtonEnableStateChanges()
        {
            IDisposable joiningRoomOperation = null;

            assertButtonEnableState(true);

            AddStep("begin joining room", () => joiningRoomOperation = OngoingOperationTracker.BeginOperation());
            assertButtonEnableState(false);

            AddStep("end joining room", () => joiningRoomOperation.Dispose());
            assertButtonEnableState(true);

            AddStep("disconnect client", () => MultiplayerClient.Disconnect());
            assertButtonEnableState(false);

            AddStep("re-connect client", () => MultiplayerClient.Connect());
            assertButtonEnableState(true);
        }

        private void assertButtonEnableState(bool enabled)
            => AddAssert($"button {(enabled ? "enabled" : "disabled")}", () => button.Enabled.Value == enabled);
    }
}
