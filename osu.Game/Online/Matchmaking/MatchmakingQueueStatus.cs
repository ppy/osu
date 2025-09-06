// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online.Matchmaking
{
    [Serializable]
    [MessagePackObject]
    [Union(0, typeof(Searching))] // IMPORTANT: Add rules to SignalRUnionWorkaroundResolver for new derived types.
    [Union(1, typeof(MatchFound))]
    [Union(2, typeof(JoiningMatch))]
    public abstract class MatchmakingQueueStatus
    {
        [Serializable]
        [MessagePackObject]
        public class Searching : MatchmakingQueueStatus
        {
        }

        [Serializable]
        [MessagePackObject]
        public class MatchFound : MatchmakingQueueStatus
        {
        }

        [Serializable]
        [MessagePackObject]
        public class JoiningMatch : MatchmakingQueueStatus
        {
        }
    }
}
