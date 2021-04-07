// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay.Components;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayer : ScreenTestScene
    {
        private TestMultiplayer multiplayerScreen;

        public TestSceneMultiplayer()
        {
            AddStep("show", () =>
            {
                multiplayerScreen = new TestMultiplayer();

                // Needs to be added at a higher level since the multiplayer screen becomes non-current.
                Child = multiplayerScreen.Client;

                LoadScreen(multiplayerScreen);
            });

            AddUntilStep("wait for loaded", () => multiplayerScreen.IsLoaded);
        }

        private class TestMultiplayer : Screens.OnlinePlay.Multiplayer.Multiplayer
        {
            [Cached(typeof(StatefulMultiplayerClient))]
            public readonly TestMultiplayerClient Client;

            public TestMultiplayer()
            {
                Client = new TestMultiplayerClient((TestMultiplayerRoomManager)RoomManager);
            }

            protected override RoomManager CreateRoomManager() => new TestMultiplayerRoomManager();
        }
    }
}
