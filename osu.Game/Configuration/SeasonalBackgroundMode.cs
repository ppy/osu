// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Configuration
{
    public enum SeasonalBackgroundMode
    {
        /// <summary>
        /// Seasonal backgrounds are shown regardless of season, if at all available.
        /// </summary>
        [LocalisableDescription(typeof(CommonStrings), nameof(CommonStrings.Always))]
        Always,

        /// <summary>
        /// Seasonal backgrounds are shown only during their corresponding season.
        /// </summary>
        [LocalisableDescription(typeof(SeasonalBackgroundModeStrings), nameof(SeasonalBackgroundModeStrings.Sometimes))]
        Sometimes,

        /// <summary>
        /// Seasonal backgrounds are never shown.
        /// </summary>
        [LocalisableDescription(typeof(CommonStrings), nameof(CommonStrings.Never))]
        Never
    }
}
