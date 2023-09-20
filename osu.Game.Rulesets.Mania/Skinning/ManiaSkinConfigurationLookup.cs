// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mania.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class ManiaSkinConfigurationLookup
    {
        /// <summary>
        /// The configuration lookup value.
        /// </summary>
        public readonly LegacyManiaSkinConfigurationLookups Lookup;

        /// <summary>
        /// The column which is being looked up.
        /// May be null if the configuration does not apply to a <see cref="Column"/>.
        /// Note that this is the absolute index across all stages.
        /// </summary>
        public readonly int? ColumnIndex;

        /// <summary>
        /// Creates a new <see cref="ManiaSkinConfigurationLookup"/>.
        /// </summary>
        /// <param name="lookup">The lookup value.</param>
        /// <param name="columnIndex">The intended <see cref="Column"/> index for the configuration. May be null if the configuration does not apply to a <see cref="Column"/>.</param>
        public ManiaSkinConfigurationLookup(LegacyManiaSkinConfigurationLookups lookup, int? columnIndex = null)
        {
            Lookup = lookup;
            ColumnIndex = columnIndex;
        }

        public override string ToString() => $"[{nameof(ManiaSkinConfigurationLookup)} lookup:{Lookup} col:{ColumnIndex}]";
    }
}
