// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// A countdown that indicates the current multiplayer server is shutting down.
    /// </summary>
    [MessagePackObject]
    public class ServerShuttingDownCountdown : MultiplayerCountdown
    {
    }
}
