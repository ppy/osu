// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics.Containers;
using OpenTK;
using osu.Framework.Graphics;

namespace osu.Game.Graphics.Processing
{
    internal class RatioAdjust : Container
    {
        public RatioAdjust()
        {
            RelativeSizeAxes = Axes.Both;
        }

        protected override void Update()
        {
            base.Update();

            Vector2 parent = Parent.DrawSize;

            // return if parent doesn't have a size yet.
            // this was happening at the top-level and causing nothing to be displayed as a result.
            if (parent.X == 0 || parent.Y == 0) return;

            Scale = new Vector2(Math.Min(parent.Y / 768f, parent.X / 1024f));
            Size = new Vector2(1 / Scale.X);
        }
    }
}
