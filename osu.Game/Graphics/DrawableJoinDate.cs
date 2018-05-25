using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Game.Graphics
{
    public class DrawableJoinDate : DrawableDate
    {
        private readonly DateTimeOffset date;

        public DrawableJoinDate(DateTimeOffset date) : base(date)
        {
            this.date = date;
        }

        protected override string Format() => Text = string.Format("{0:MMMM yyyy}", date);

        public override string TooltipText => string.Format("{0:d MMMM yyyy}", date);
    }
}
