// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online.Multiplayer.MatchTypes.RankedPlay
{
    public enum RankedPlayStage
    {
        /// <summary>
        /// Waiting for clients to join.
        /// </summary>
        WaitForJoin,

        /// <summary>
        /// Period of time before the round starts.
        /// </summary>
        RoundWarmup,

        /// <summary>
        /// Users are discarding cards and drawing new ones.
        /// </summary>
        CardDiscard,

        /// <summary>
        /// Users have finished discarding their cards.
        /// </summary>
        FinishCardDiscard,

        /// <summary>
        /// The active user is selecting a card to play.
        /// </summary>
        CardPlay,

        /// <summary>
        /// The active user has made a selection, both players should now start downloading it.
        /// </summary>
        FinishCardPlay,

        /// <summary>
        /// Period of time before gameplay starts.
        /// </summary>
        GameplayWarmup,

        /// <summary>
        /// Gameplay is in progress.
        /// </summary>
        Gameplay,

        /// <summary>
        /// Users are viewing the gameplay results
        /// </summary>
        Results,

        /// <summary>
        /// The match has concluded.
        /// </summary>
        Ended
    }
}
