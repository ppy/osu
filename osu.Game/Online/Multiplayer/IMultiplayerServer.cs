// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// An interface defining the multiplayer server instance.
    /// </summary>
    public interface IMultiplayerServer : IMultiplayerRoomServer, IMultiplayerLoungeServer
    {
    }
}
