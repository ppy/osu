// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Scoring;

namespace osu.Game.Screens.SelectV2
{
    /// <summary>
    /// Actions exposed by song select which are used by subcomponents to perform top-level operations.
    /// </summary>
    public interface ISongSelect
    {
        /// <summary>
        /// Requests the user for confirmation to delete the given beatmap set.
        /// </summary>
        void Delete(BeatmapSetInfo beatmapBeatmapSetInfo);

        /// <summary>
        /// Immediately restores any hidden beatmaps in the provided beatmap set.
        /// </summary>
        void RestoreAllHidden(BeatmapSetInfo beatmapSet);

        /// <summary>
        /// Opens the manage collections dialog.
        /// </summary>
        void ManageCollections();

        /// <summary>
        /// Opens results screen with the given score.
        /// This assumes active beatmap and ruleset selection matches the score.
        /// </summary>
        void PresentScore(ScoreInfo score);

        /// <summary>
        /// Set the current filter text query to the provided string.
        /// </summary>
        void Search(string query);

        /// <summary>
        /// Gets relevant actionable items for beatmap context menus, based on the type of song select.
        /// </summary>
        IEnumerable<OsuMenuItem> GetForwardActions(BeatmapInfo beatmap);
    }
}
