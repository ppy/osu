// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Ranking
{
    public class AspectContainer : Container
    {
        protected override void Update()
        {
            base.Update();
            if (RelativeSizeAxes == Axes.X)
                Height = DrawWidth;
            else
                Width = DrawHeight;
        }
    }
}
