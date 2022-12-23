// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Game.Scoring;

namespace osu.Game.Online.Solo
{
    /// <summary>
    /// A component that delivers updates to the logged in user's gameplay statistics after completed scores.
    /// </summary>
    [Cached]
    public interface ISoloStatisticsWatcher
    {
        /// <summary>
        /// Registers for a user statistics update after the given <paramref name="score"/> has been processed server-side.
        /// </summary>
        /// <param name="score">The score to listen for the statistics update for.</param>
        /// <param name="onUpdateReady">The callback to be invoked once the statistics update has been prepared.</param>
        void RegisterForStatisticsUpdateAfter(ScoreInfo score, Action<SoloStatisticsUpdate> onUpdateReady);
    }
}
