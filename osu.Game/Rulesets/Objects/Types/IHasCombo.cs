// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject that is part of a combo.
    /// </summary>
    public interface IHasCombo
    {
        /// <summary>
        /// Whether the HitObject starts a new combo.
        /// </summary>
        bool NewCombo { get; }

        /// <summary>
        /// When starting a new combo, the offset of the new combo relative to the current one.
        /// </summary>
        /// <remarks>
        /// This is generally a setting provided by a beatmap creator to choreograph interesting colour patterns
        /// which can only be achieved by skipping combo colours with per-hitobject level.
        ///
        /// It is exposed via <see cref="IHasComboInformation.ComboIndexWithOffsets"/>.
        /// </remarks>
        int ComboOffset { get; }
    }
}
