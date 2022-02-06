// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Skinning;

namespace osu.Game.Screens.Edit
{
    public class NewDifficultyCreationParameters
    {
        /// <summary>
        /// The <see cref="BeatmapSetInfo"/> that should contain the newly-created difficulty.
        /// </summary>
        public BeatmapSetInfo BeatmapSet { get; }

        /// <summary>
        /// The <see cref="RulesetInfo"/> that the new difficulty should be playable for.
        /// </summary>
        public RulesetInfo Ruleset { get; }

        /// <summary>
        /// A reference <see cref="IBeatmap"/> upon which the new difficulty should be based.
        /// </summary>
        public IBeatmap ReferenceBeatmap { get; }

        /// <summary>
        /// A reference <see cref="ISkin"/> that the new difficulty should base its own skin upon.
        /// </summary>
        public ISkin? ReferenceBeatmapSkin { get; }

        /// <summary>
        /// Whether the new difficulty should be blank.
        /// </summary>
        /// <remarks>
        /// A blank difficulty will have no objects, no control points other than timing points taken from <see cref="ReferenceBeatmap"/>
        /// and will not share <see cref="BeatmapInfo"/> values with <see cref="ReferenceBeatmap"/>,
        /// but it will share metadata and timing information with <see cref="ReferenceBeatmap"/>.
        /// </remarks>
        public bool CreateBlank { get; }

        /// <summary>
        /// The saved state of the previous <see cref="Editor"/> which should be restored upon opening the newly-created difficulty.
        /// </summary>
        public EditorState EditorState { get; }

        public NewDifficultyCreationParameters(
            BeatmapSetInfo beatmapSet,
            RulesetInfo ruleset,
            IBeatmap referenceBeatmap,
            ISkin? referenceBeatmapSkin,
            bool createBlank,
            EditorState editorState)
        {
            BeatmapSet = beatmapSet;
            Ruleset = ruleset;
            ReferenceBeatmap = referenceBeatmap;
            ReferenceBeatmapSkin = referenceBeatmapSkin;
            CreateBlank = createBlank;
            EditorState = editorState;
        }
    }
}
