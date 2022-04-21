// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// A <see cref="MultiplayerCountdown"/> indicating that gameplay will start after a given period of time.
    /// </summary>
    [MessagePackObject]
    public class GameplayStartCountdown : MultiplayerCountdown
    {
    }
}
