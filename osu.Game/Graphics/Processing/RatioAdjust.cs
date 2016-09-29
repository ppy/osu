//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
            Scale = new Vector2(Parent.ActualSize.Y / 768f);
            Size = new Vector2(1 / Scale.X);
        }
    }
}
