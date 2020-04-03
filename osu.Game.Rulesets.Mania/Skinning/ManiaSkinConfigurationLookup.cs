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
        /// The intended <see cref="Column"/> index for the configuration.
        /// May be null if the configuration does not apply to a <see cref="Column"/>.
        /// </summary>
        public readonly int? TargetColumn;

        /// <summary>
        /// Creates a new <see cref="ManiaSkinConfigurationLookup"/>.
        /// </summary>
        /// <param name="lookup">The lookup value.</param>
        /// <param name="targetColumn">The intended <see cref="Column"/> index for the configuration. May be null if the configuration does not apply to a <see cref="Column"/>.</param>
        public ManiaSkinConfigurationLookup(LegacyManiaSkinConfigurationLookups lookup, int? targetColumn = null)
        {
            Lookup = lookup;
            TargetColumn = targetColumn;
        }
    }
}
