// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class MultiplayerSubScreenTestScene : OnlinePlayTestScene, IMultiplayerRoomTestDependencies
    {
        public TestMultiplayerClient Client => RoomDependencies.Client;
        public new TestMultiplayerRoomManager RoomManager => RoomDependencies.RoomManager;

        protected new MultiplayerRoomTestDependencies RoomDependencies => (MultiplayerRoomTestDependencies)base.RoomDependencies;

        protected override RoomTestDependencies CreateRoomDependencies() => new MultiplayerRoomTestDependencies();
    }
}
