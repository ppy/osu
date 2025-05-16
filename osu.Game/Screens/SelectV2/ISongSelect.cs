// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
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
        /// Requests the user for confirmation to clear all local scores in the given beatmap.
        /// </summary>
        void ClearScores(BeatmapInfo beatmap);

        /// <summary>
        /// Opens beatmap editor with the given beatmap.
        /// </summary>
        void Edit(BeatmapInfo beatmap);

        /// <summary>
        /// Whether calls to <see cref="Edit"/> will succeed or not.
        /// </summary>
        bool EditingAllowed { get; }

        /// <summary>
        /// Opens the manage collections dialog.
        /// </summary>
        void ManageCollections();

        /// <summary>
        /// Marks a beatmap manually as being played.
        /// </summary>
        void MarkPlayed(BeatmapInfo beatmap);

        /// <summary>
        /// Hides a beatmap from user's vision.
        /// </summary>
        void Hide(BeatmapInfo beatmap);

        /// <summary>
        /// Present the provided score at the results screen.
        /// </summary>
        void PresentScore(ScoreInfo score);

        /// <summary>
        /// Selects the provided beatmap and progresses song select to the next screen.
        /// </summary>
        void SelectAndStart(BeatmapInfo beatmap);
    }
}
