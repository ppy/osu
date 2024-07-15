// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// A <see cref="MultiplayerCountdown"/> started by the server when clients being to load.
    /// Indicates how long until gameplay will forcefully start, excluding any users which have not completed loading,
    /// and forcing progression of any clients that are blocking load due to user interaction.
    /// </summary>
    [MessagePackObject]
    public sealed class ForceGameplayStartCountdown : MultiplayerCountdown
    {
    }
}
