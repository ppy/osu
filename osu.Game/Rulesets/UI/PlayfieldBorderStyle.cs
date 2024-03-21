// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Rulesets.UI
{
    public enum PlayfieldBorderStyle
    {
        [LocalisableDescription(typeof(RulesetSettingsStrings), nameof(RulesetSettingsStrings.BorderNone))]
        None,

        [LocalisableDescription(typeof(RulesetSettingsStrings), nameof(RulesetSettingsStrings.BorderCorners))]
        Corners,

        [LocalisableDescription(typeof(RulesetSettingsStrings), nameof(RulesetSettingsStrings.BorderFull))]
        Full
    }
}
