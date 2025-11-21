// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Rulesets.Mania.Configuration;

namespace osu.Game.Rulesets.Mania
{
    public enum ManiaMobileLayout
    {
        [LocalisableDescription(typeof(RulesetSettingsStrings), nameof(RulesetSettingsStrings.Portrait))]
        Portrait,

        [LocalisableDescription(typeof(RulesetSettingsStrings), nameof(RulesetSettingsStrings.Landscape))]
        Landscape,

        [LocalisableDescription(typeof(RulesetSettingsStrings), nameof(RulesetSettingsStrings.LandscapeExpandedColumns))]
        LandscapeExpandedColumns,

        [Obsolete($"Use {nameof(ManiaRulesetSetting.TouchOverlay)} instead.")] // todo: can be removed 20260211
        LandscapeWithOverlay,
    }
}
