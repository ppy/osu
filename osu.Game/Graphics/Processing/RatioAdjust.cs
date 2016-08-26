//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT License - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;

namespace osu.Game.Graphics.Processing
{
    class RatioAdjust : LargeContainer
    {
        protected override void Update()
        {
            base.Update();
            Scale = Parent.ActualSize.Y / 768f;
            Size = new Vector2(1 / Scale);
        }
    }
}
