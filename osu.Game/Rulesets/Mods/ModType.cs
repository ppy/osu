// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Rulesets.Mods
{
    public enum ModType
    {
        [LocalisableDescription(typeof(ModSelectOverlayStrings), nameof(ModSelectOverlayStrings.DifficultyReduction))]
        DifficultyReduction,

        [LocalisableDescription(typeof(ModSelectOverlayStrings), nameof(ModSelectOverlayStrings.DifficultyIncrease))]
        DifficultyIncrease,

        [LocalisableDescription(typeof(ModSelectOverlayStrings), nameof(ModSelectOverlayStrings.Conversion))]
        Conversion,

        [LocalisableDescription(typeof(ModSelectOverlayStrings), nameof(ModSelectOverlayStrings.Automation))]
        Automation,

        [LocalisableDescription(typeof(ModSelectOverlayStrings), nameof(ModSelectOverlayStrings.Fun))]
        Fun,
        System // Doesn't need to be localized as it won't be seen anywher afaik
    }
}
