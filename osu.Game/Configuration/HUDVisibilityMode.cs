// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Configuration
{
    public enum HUDVisibilityMode
    {
        [LocalisableDescription(typeof(CommonStrings), nameof(CommonStrings.Never))]
        Never,

        [LocalisableDescription(typeof(HUDVisibilityModeStrings), nameof(HUDVisibilityModeStrings.HideDuringGameplay))]
        HideDuringGameplay,

        [LocalisableDescription(typeof(CommonStrings), nameof(CommonStrings.Always))]
        Always
    }
}
