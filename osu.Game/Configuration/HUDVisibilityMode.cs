// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Configuration
{
    public enum HUDVisibilityMode
    {
        [LocalisableDescription(typeof(GameplaySettingsStrings), nameof(GameplaySettingsStrings.NeverShowHUD))]
        Never,

        [LocalisableDescription(typeof(GameplaySettingsStrings), nameof(GameplaySettingsStrings.HideDuringGameplay))]
        HideDuringGameplay,

        [LocalisableDescription(typeof(GameplaySettingsStrings), nameof(GameplaySettingsStrings.AlwaysShowHUD))]
        Always
    }
}
