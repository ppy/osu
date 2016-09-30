//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics.Containers;
using OpenTK;

namespace osu.Game.Graphics.Processing
{
    class RatioAdjust : LargeContainer
    {
        public override bool Contains(Vector2 screenSpacePos) => true;

        protected override void Update()
        {
            base.Update();
            Vector2 parent = Parent.ActualSize;

            Scale = new Vector2(Math.Min(parent.Y / 768f, parent.X / 1024f));
            Size = new Vector2(1 / Scale.X);
        }
    }
}
