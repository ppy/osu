// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Skinning
{
    public class SkinComboColourLookup
    {
        /// <summary>
        /// The index to use for deciding the combo colour.
        /// </summary>
        public readonly int ColourIndex;

        /// <summary>
        /// The combo information requesting the colour.
        /// </summary>
        public readonly IHasComboInformation Combo;

        public SkinComboColourLookup(int colourIndex, IHasComboInformation combo)
        {
            ColourIndex = colourIndex;
            Combo = combo;
        }
    }
}
