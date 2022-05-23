// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        public string Content;
        public LocalisableString Name;
    }
}
