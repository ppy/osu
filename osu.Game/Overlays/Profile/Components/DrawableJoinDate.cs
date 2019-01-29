// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Profile.Components
{
    public class DrawableJoinDate : DrawableDate
    {
        public DrawableJoinDate(DateTimeOffset date)
            : base(date)
        {
        }

        protected override string Format() => Text = Date.ToUniversalTime().Year < 2008 ? "Here since the beginning" : $"{Date:MMMM yyyy}";

        public override string TooltipText => $"{Date:MMMM d, yyyy}";
    }
}
