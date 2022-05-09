// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Online.Broadcasts
{
    public interface IGameStateBroadcastServer
    {
        void Add(GameStateBroadcaster broadcaster);
        void AddRange(IEnumerable<GameStateBroadcaster> broadcasters);
        void Remove(GameStateBroadcaster broadcaster);
        void Broadcast(string message);
        void Broadcast(ReadOnlyMemory<byte> message);
    }
}
