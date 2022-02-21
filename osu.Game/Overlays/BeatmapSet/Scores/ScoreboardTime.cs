// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Graphics;
using osu.Game.Utils;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoreboardTime : DrawableDate
    {
        public ScoreboardTime(DateTimeOffset date, float textSize = OsuFont.DEFAULT_FONT_SIZE, bool italic = true)
            : base(date, textSize, italic)
        {
        }

        protected override string Format()
            => ScoreboardTimeUtils.FormatRelativeTime(Date, TimeSpan.FromHours(1));
    }
}
