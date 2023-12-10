// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Skinning
{
    public class SkinSnapColourLookup
    {
        /// <summary>
        /// The index to use for deciding the snap colour.
        /// </summary>
        public readonly int ColourIndex;

        /// <summary>
        /// The snap information requesting the colour.
        /// </summary>
        public readonly IHasSnapInformation Snap;

        public SkinSnapColourLookup(int colourIndex, IHasSnapInformation snap)
        {
            ColourIndex = colourIndex;
            Snap = snap;
        }
    }
}
