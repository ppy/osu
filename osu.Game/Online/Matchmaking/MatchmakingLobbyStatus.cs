// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online.Matchmaking
{
    [Serializable]
    [MessagePackObject]
    public class MatchmakingLobbyStatus
    {
        [Key(0)]
        public int[] UsersInQueue { get; set; } = [];
    }
}
