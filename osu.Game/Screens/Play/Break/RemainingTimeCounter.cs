// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Play.Break
{
    public class RemainingTimeCounter : Counter
    {
        private readonly OsuSpriteText counter;

        public RemainingTimeCounter()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = counter = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                TextSize = 33,
                Font = "Venera",
            };
        }

        protected override void OnCountChanged(double count) => counter.Text = ((int)Math.Ceiling(count / 1000)).ToString();
    }
}
