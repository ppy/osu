// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Configuration
{
    public enum RandomSelectAlgorithm
    {
        [LocalisableDescription(typeof(RandomSelectAlgorithmStrings), nameof(RandomSelectAlgorithmStrings.NeverRepeat))]
        RandomPermutation,

        [LocalisableDescription(typeof(RandomSelectAlgorithmStrings), nameof(RandomSelectAlgorithmStrings.TrueRandom))]
        Random
    }
}
