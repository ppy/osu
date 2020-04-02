// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class ManiaSkinConfigurationLookup
    {
        public readonly LegacyManiaSkinConfigurationLookups Lookup;
        public readonly int? TargetColumn;

        public ManiaSkinConfigurationLookup(LegacyManiaSkinConfigurationLookups lookup, int? targetColumn = null)
        {
            Lookup = lookup;
            TargetColumn = targetColumn;
        }
    }
}
