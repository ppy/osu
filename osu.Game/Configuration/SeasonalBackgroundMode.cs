// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Configuration
{
    public enum SeasonalBackgroundMode
    {
        /// <summary>
        /// Seasonal backgrounds are shown regardless of season, if at all available.
        /// </summary>
        [LocalisableDescription(typeof(UserInterfaceStrings), nameof(UserInterfaceStrings.AlwaysSeasonalBackground))]
        Always,

        /// <summary>
        /// Seasonal backgrounds are shown only during their corresponding season.
        /// </summary>
        [LocalisableDescription(typeof(UserInterfaceStrings), nameof(UserInterfaceStrings.SometimesSeasonalBackground))]
        Sometimes,

        /// <summary>
        /// Seasonal backgrounds are never shown.
        /// </summary>
        [LocalisableDescription(typeof(UserInterfaceStrings), nameof(UserInterfaceStrings.NeverSeasonalBackground))]
        Never
    }
}
