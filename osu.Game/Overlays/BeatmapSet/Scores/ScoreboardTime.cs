// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Extensions;
using osu.Game.Graphics;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public partial class ScoreboardTime : DrawableDate
    {
        public ScoreboardTime(DateTimeOffset date, float textSize = OsuFont.DEFAULT_FONT_SIZE, bool italic = true)
            : base(date, textSize, italic)
        {
        }

        protected override string Format()
            => Date.ToShortRelativeTime(TimeSpan.FromHours(1));
    }
}
