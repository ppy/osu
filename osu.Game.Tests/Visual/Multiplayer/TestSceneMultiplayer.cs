// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay.Components;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayer : ScreenTestScene
    {
        public TestSceneMultiplayer()
        {
            var multi = new TestMultiplayer();

            AddStep("show", () => LoadScreen(multi));
            AddUntilStep("wait for loaded", () => multi.IsLoaded);
        }

        private class TestMultiplayer : Screens.OnlinePlay.Multiplayer.Multiplayer
        {
            [Cached(typeof(StatefulMultiplayerClient))]
            public readonly TestMultiplayerClient Client;

            public TestMultiplayer()
            {
                AddInternal(Client = new TestMultiplayerClient((TestMultiplayerRoomManager)RoomManager));
            }

            protected override RoomManager CreateRoomManager() => new TestMultiplayerRoomManager();
        }
    }
}
