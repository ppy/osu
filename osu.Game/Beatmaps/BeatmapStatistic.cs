// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace osu.Game.Beatmaps
{
    public class BeatmapStatistic
    {
        [Obsolete("Use CreateIcon instead")] // can be removed 20210203
        public IconUsage Icon = FontAwesome.Regular.QuestionCircle;

        /// <summary>
        /// A function to create the icon for display purposes. Use default icons available via <see cref="BeatmapStatisticIcon"/> whenever possible for conformity.
        /// </summary>
        public Func<Drawable> CreateIcon;

        public string Content;
        public string Name;

        public BeatmapStatistic()
        {
#pragma warning disable 618
            CreateIcon = () => new SpriteIcon { Icon = Icon, Scale = new Vector2(0.7f) };
#pragma warning restore 618
        }
    }
}
