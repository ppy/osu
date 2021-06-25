// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public interface IMultiplayerRoomTestDependencies : IRoomTestDependencies
    {
        /// <summary>
        /// The cached <see cref="MultiplayerClient"/>.
        /// </summary>
        TestMultiplayerClient Client { get; }

        /// <summary>
        /// The cached <see cref="IRoomManager"/>.
        /// </summary>
        new TestMultiplayerRoomManager RoomManager { get; }
    }
}
