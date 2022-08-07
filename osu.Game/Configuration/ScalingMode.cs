// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Configuration
{
    public enum ScalingMode
    {
        [LocalisableDescription(typeof(ScalingModeStrings), nameof(ScalingModeStrings.Off))]
        Off,

        [LocalisableDescription(typeof(ScalingModeStrings), nameof(ScalingModeStrings.Everything))]
        Everything,

        [LocalisableDescription(typeof(ScalingModeStrings), nameof(ScalingModeStrings.ExcludingOverlays))]
        ExcludeOverlays,

        [LocalisableDescription(typeof(ScalingModeStrings), nameof(ScalingModeStrings.Gameplay))]
        Gameplay,
    }
}
