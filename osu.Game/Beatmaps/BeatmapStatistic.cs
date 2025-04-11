// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Graphics;
using osu.Framework.Localisation;

namespace osu.Game.Beatmaps
{
    public class BeatmapStatistic
    {
        /// <summary>
        /// A function to create the icon for display purposes. Use default icons available via <see cref="BeatmapStatisticIcon"/> whenever possible for conformity.
        /// </summary>
        public Func<Drawable> CreateIcon;

        /// <summary>
        /// The name of this statistic.
        /// </summary>
        public LocalisableString Name;

        /// <summary>
        /// The text representing the value of this statistic.
        /// </summary>
        public string Content;

        /// <summary>
        /// The ratio of this statistic compared to other relevant statistics, or null if not applicable.
        /// </summary>
        /// <remarks>
        /// This is used to display a bar on top of the statistic with the given ratio.
        /// </remarks>
        public float? Ratio;
    }
}
