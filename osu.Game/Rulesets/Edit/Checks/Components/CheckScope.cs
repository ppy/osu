// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Rulesets.Edit.Checks.Components
{
    public enum CheckScope
    {
        /// <summary>
        /// Run checks that apply to the current difficulty.
        /// </summary>
        [LocalisableDescription(typeof(EditorStrings), nameof(EditorStrings.CheckCurrentDifficulty))]
        Difficulty,

        /// <summary>
        /// Run checks that apply to the beatmap set as a whole.
        /// </summary>
        [LocalisableDescription(typeof(EditorStrings), nameof(EditorStrings.CheckEntireBeatmapSet))]
        BeatmapSet,
    }
}
