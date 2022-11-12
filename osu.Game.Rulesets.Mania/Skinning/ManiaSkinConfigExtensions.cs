// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public static class ManiaSkinConfigExtensions
    {
        /// <summary>
        /// Retrieve a per-column-count skin configuration.
        /// </summary>
        /// <param name="skin">The skin from which configuration is retrieved.</param>
        /// <param name="lookup">The value to retrieve.</param>
        /// <param name="columnIndex">If not null, denotes the index of the column to which the entry applies.</param>
        public static IBindable<T>? GetManiaSkinConfig<T>(this ISkin skin, LegacyManiaSkinConfigurationLookups lookup, int? columnIndex = null)
            where T : notnull
            => skin.GetConfig<ManiaSkinConfigurationLookup, T>(new ManiaSkinConfigurationLookup(lookup, columnIndex));
    }
}
