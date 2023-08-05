// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Configuration
{
    public enum RandomSelectAlgorithm
    {
        [LocalisableDescription(typeof(UserInterfaceStrings), nameof(UserInterfaceStrings.NeverRepeat))]
        RandomPermutation,

        [LocalisableDescription(typeof(UserInterfaceStrings), nameof(UserInterfaceStrings.TrueRandom))]
        Random
    }
}
