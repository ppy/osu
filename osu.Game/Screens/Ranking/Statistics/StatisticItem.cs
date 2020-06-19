// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Ranking.Statistics
{
    public class StatisticItem
    {
        public readonly string Name;
        public readonly Drawable Content;
        public readonly Dimension Dimension;

        public StatisticItem([NotNull] string name, [NotNull] Drawable content, [CanBeNull] Dimension dimension = null)
        {
            Name = name;
            Content = content;
            Dimension = dimension;
        }
    }
}
