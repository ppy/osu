// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Configuration
{
    public enum IntroSequence
    {
        Circles,
        Welcome,
        Triangles,

        [LocalisableDescription(typeof(UserInterfaceStrings), nameof(UserInterfaceStrings.IntroRandom))]
        Random
    }
}
