// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets;

namespace osu.Game.Screens.Edit
{
    public class NewDifficultyCreationParameters
    {
        /// <summary>
        /// The <see cref="BeatmapSetInfo"/> that should contain the newly-created difficulty.
        /// </summary>
        public BeatmapSetInfo BeatmapSet { get; set; }

        /// <summary>
        /// The <see cref="RulesetInfo"/> that the new difficulty should be playable for.
        /// </summary>
        public RulesetInfo Ruleset { get; set; }

        /// <summary>
        /// A reference <see cref="IBeatmap"/> upon which the new difficulty should be based.
        /// </summary>
        public IBeatmap ReferenceBeatmap { get; set; }

        /// <summary>
        /// Whether all objects should be cleared from the new difficulty.
        /// </summary>
        public bool ClearAllObjects { get; set; }

        /// <summary>
        /// The saved state of the previous <see cref="Editor"/> which should be restored upon opening the newly-created difficulty.
        /// </summary>
        public EditorState EditorState { get; set; }
    }
}
