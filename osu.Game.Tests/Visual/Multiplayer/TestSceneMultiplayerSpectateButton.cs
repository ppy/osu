// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerSpectateButton : MultiplayerTestScene
    {
        private MultiplayerSpectateButton button;
        private IDisposable readyClickOperation;

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            Child = button = new MultiplayerSpectateButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(200, 50),
                OnSpectateClick = async () =>
                {
                    readyClickOperation = OngoingOperationTracker.BeginOperation();
                    await Client.ToggleSpectate();
                    readyClickOperation.Dispose();
                }
            };
        });

        [TestCase(MultiplayerUserState.Idle)]
        [TestCase(MultiplayerUserState.Ready)]
        public void TestToggleWhenIdle(MultiplayerUserState initialState)
        {
            addClickButtonStep();
            AddAssert("user is spectating", () => Client.Room?.Users[0].State == MultiplayerUserState.Spectating);

            addClickButtonStep();
            AddAssert("user is idle", () => Client.Room?.Users[0].State == MultiplayerUserState.Idle);
        }

        private void addClickButtonStep() => AddStep("click button", () =>
        {
            InputManager.MoveMouseTo(button);
            InputManager.Click(MouseButton.Left);
        });
    }
}
