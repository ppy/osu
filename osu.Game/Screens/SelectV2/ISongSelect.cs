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
        /// Opens the manage collections dialog.
        /// </summary>
        void ManageCollections();

        /// <summary>
        /// Present the provided score at the results screen.
        /// </summary>
        void PresentScore(ScoreInfo score);

        /// <summary>
        /// Gets relevant actionable items for beatmap context menus, based on the type of song select.
        /// </summary>
        IEnumerable<OsuMenuItem> GetForwardActions(BeatmapInfo beatmap);
    }
}
