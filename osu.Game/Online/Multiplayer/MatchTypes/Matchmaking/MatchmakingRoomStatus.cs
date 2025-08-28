// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Online.Multiplayer.MatchTypes.Matchmaking
{
    /// <summary>
    /// Describes the current status of a matchmaking room.
    /// </summary>
    [Serializable]
    public enum MatchmakingRoomStatus
    {
        /// <summary>
        /// Room starts. Some clients may still be joining.
        /// </summary>
        RoomStart,

        /// <summary>
        /// Round starts.
        /// </summary>
        RoundStart,

        /// <summary>
        /// Clients pick beatmaps.
        /// </summary>
        UserPicks,

        /// <summary>
        /// Server selects the next beatmap.
        /// </summary>
        SelectBeatmap,

        /// <summary>
        /// Gameplay beatmap is revealed - wait for clients to download and set the beatmap.
        /// </summary>
        PrepareBeatmap,

        /// <summary>
        /// Preview time before gameplay starts.
        /// </summary>
        PrepareGameplay,

        /// <summary>
        /// Gameplay starts.
        /// </summary>
        Gameplay,

        /// <summary>
        /// Round ends. Some clients may still be viewing results.
        /// </summary>
        RoundEnd,

        /// <summary>
        /// Room ends. Some clients may still be chatting.
        /// </summary>
        RoomEnd
    }
}
