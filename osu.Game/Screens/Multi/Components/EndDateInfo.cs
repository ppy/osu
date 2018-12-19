// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Graphics;

namespace osu.Game.Screens.Multi.Components
{
    public class EndDateInfo : DrawableDate
    {
        public EndDateInfo()
            : base(DateTimeOffset.UtcNow)
        {
        }

        protected override string Format()
        {
            var diffToNow = Date.Subtract(DateTimeOffset.Now);

            if (diffToNow.TotalSeconds < -5)
                return $"Closed {base.Format()}";

            if (diffToNow.TotalSeconds < 0)
                return "Closed";

            if (diffToNow.TotalSeconds < 5)
                return "Closing soon";

            return $"Closing {base.Format()}";
        }
    }
}
