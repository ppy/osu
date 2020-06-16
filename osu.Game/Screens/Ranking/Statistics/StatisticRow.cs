// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Ranking.Statistics
{
    public class StatisticRow
    {
        public Drawable[] Content = Array.Empty<Drawable>();
        public Dimension[] ColumnDimensions = Array.Empty<Dimension>();
    }
}
