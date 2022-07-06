// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using osu.Game.Online.Multiplayer.Countdown;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// An event from the server to allow clients to update gameplay to an expected state.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    // IMPORTANT: Add rules to SignalRUnionWorkaroundResolver for new derived types.
    [Union(0, typeof(CountdownChangedEvent))]
    public abstract class MatchServerEvent
    {
    }
}
