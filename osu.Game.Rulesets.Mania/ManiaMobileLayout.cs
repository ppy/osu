// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Rulesets.Mania
{
    public enum ManiaMobileLayout
    {
        [LocalisableDescription(typeof(RulesetSettingsStrings), nameof(RulesetSettingsStrings.PortraitExpandedColumns))]
        Portrait,

        [LocalisableDescription(typeof(RulesetSettingsStrings), nameof(RulesetSettingsStrings.LandscapeExpandedColumns))]
        Landscape,

        [LocalisableDescription(typeof(RulesetSettingsStrings), nameof(RulesetSettingsStrings.LandscapeTouchOverlay))]
        LandscapeWithOverlay,
    }
}
