// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Online.Multiplayer.MatchTypes.Matchmaking
{
    /// <summary>
    /// Describes the current status of a matchmaking room.
    /// </summary>
    [Serializable]
    public enum MatchmakingStage
    {
        /// <summary>
        /// The initial state of a room. Users are still joining.
        /// </summary>
        WaitingForClientsJoin,

        /// <summary>
        /// A short delay before the round begins.
        /// </summary>
        RoundWarmupTime,

        /// <summary>
        /// Users are given a chance to lock in their beatmap picks.
        /// </summary>
        UserBeatmapSelect,

        /// <summary>
        /// Clients have sent their picks, and the server has responded with the finalised beatmap.
        /// </summary>
        ServerBeatmapFinalised,

        /// <summary>
        /// Clients are given an opportunity to download the beatmap.
        /// </summary>
        WaitingForClientsBeatmapDownload,

        /// <summary>
        /// A short delay before gameplay starts.
        /// </summary>
        GameplayWarmupTime,

        /// <summary>
        /// Gameplay is ongoing.
        /// </summary>
        Gameplay,

        /// <summary>
        /// Gameplay has finished, results are being displayed.
        /// </summary>
        ResultsDisplaying,

        /// <summary>
        /// All rounds have completed. Users may still be chatting.
        /// </summary>
        Ended
    }
}
