// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens.Multi.Components;

namespace osu.Game.Tests.Visual.RealtimeMultiplayer
{
    public class TestSceneRealtimeMultiplayer : RealtimeMultiplayerTestScene
    {
        public TestSceneRealtimeMultiplayer()
        {
            var multi = new TestRealtimeMultiplayer();

            AddStep("show", () => LoadScreen(multi));
            AddUntilStep("wait for loaded", () => multi.IsLoaded);
        }

        private class TestRealtimeMultiplayer : Screens.Multi.RealtimeMultiplayer.RealtimeMultiplayer
        {
            protected override RoomManager CreateRoomManager() => new TestRealtimeRoomManager();
        }
    }
}
