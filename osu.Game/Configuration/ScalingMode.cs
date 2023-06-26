// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Configuration
{
    public enum ScalingMode
    {
        [LocalisableDescription(typeof(LayoutSettingsStrings), nameof(LayoutSettingsStrings.ScalingOff))]
        Off,

        [LocalisableDescription(typeof(LayoutSettingsStrings), nameof(LayoutSettingsStrings.ScaleEverything))]
        Everything,

        [LocalisableDescription(typeof(LayoutSettingsStrings), nameof(LayoutSettingsStrings.ScaleEverythingExcludingOverlays))]
        ExcludeOverlays,

        [LocalisableDescription(typeof(LayoutSettingsStrings), nameof(LayoutSettingsStrings.ScaleGameplay))]
        Gameplay,
    }
}
