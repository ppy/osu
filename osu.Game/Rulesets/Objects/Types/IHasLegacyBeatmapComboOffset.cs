// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A type of <see cref="HitObject"/> that has a combo index with arbitrary offsets applied to use when retrieving legacy beatmap combo colours.
    /// This is done in stable for hitobjects to skip combo colours from the beatmap skin (known as "colour hax").
    /// See https://osu.ppy.sh/wiki/en/osu%21_File_Formats/Osu_%28file_format%29#type for more information.
    /// </summary>
    public interface IHasLegacyBeatmapComboOffset
    {
        /// <summary>
        /// The legacy offset of the new combo relative to the current one, when starting a new combo.
        /// </summary>
        int LegacyBeatmapComboOffset { get; }

        /// <summary>
        /// The combo index with the <see cref="LegacyBeatmapComboOffset"/> applied,
        /// to use for legacy beatmap skins to decide on the combo colour.
        /// </summary>
        int LegacyBeatmapComboIndex { get; set; }
    }
}
