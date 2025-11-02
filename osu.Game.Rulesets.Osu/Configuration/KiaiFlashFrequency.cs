// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Rulesets.Osu.Configuration
{
    public enum KiaiFlashFrequency
    {
        [LocalisableDescription(typeof(RulesetSettingsStrings), nameof(RulesetSettingsStrings.KiaiFlashFrequency1x))]
        EveryBeat,

        [LocalisableDescription(typeof(RulesetSettingsStrings), nameof(RulesetSettingsStrings.KiaiFlashFrequency0_5x))]
        EverySecondBeat,

        [LocalisableDescription(typeof(RulesetSettingsStrings), nameof(RulesetSettingsStrings.KiaiFlashFrequency0_25x))]
        EveryFourthBeat,
    }
}
