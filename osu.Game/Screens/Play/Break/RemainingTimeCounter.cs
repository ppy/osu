// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Play.Break
{
    public partial class RemainingTimeCounter : Counter
    {
        private readonly OsuSpriteText counter;

        public RemainingTimeCounter()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = counter = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Numeric.With(size: 33),
            };
        }

        protected override void OnCountChanged(double count) => counter.Text = ((int)Math.Ceiling(count / 1000)).ToString();
    }
}
