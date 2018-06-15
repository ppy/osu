// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;

namespace osu.Game.Graphics
{
    public class DrawableJoinDate : DrawableDate
    {
        private readonly DateTimeOffset date;

        public DrawableJoinDate(DateTimeOffset date) : base(date)
        {
            this.date = date;
        }

        protected override string Format() => Text = string.Format($"{date:MMMM yyyy}");

        public override string TooltipText => string.Format($"{date:MMMM d, yyyy}");
    }
}
