// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Configuration
{
    public enum ScalingMode
    {
        [LocalisableDescription(typeof(LayoutSettingsStrings), nameof(LayoutSettingsStrings.Off))]
        Off,

        [LocalisableDescription(typeof(LayoutSettingsStrings), nameof(LayoutSettingsStrings.Everything))]
        Everything,

        [LocalisableDescription(typeof(LayoutSettingsStrings), nameof(LayoutSettingsStrings.ExcludingOverlays))]
        ExcludeOverlays,

        [LocalisableDescription(typeof(LayoutSettingsStrings), nameof(LayoutSettingsStrings.Gameplay))]
        Gameplay,
    }
}
