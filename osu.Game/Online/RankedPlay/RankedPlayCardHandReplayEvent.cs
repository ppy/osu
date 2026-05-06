// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Online.RankedPlay
{
    [Serializable]
    [MessagePackObject]
    public class RankedPlayCardHandReplayEvent : MatchServerEvent
    {
        /// <summary>
        /// The user performing the action.
        /// </summary>
        [Key(0)]
        public int UserId { get; set; }

        [Key(1)]
        public required RankedPlayCardHandReplayFrame[] Frames { get; init; }
    }
}
