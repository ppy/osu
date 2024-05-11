// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Ranking
{
    public partial class AspectContainer : Container
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
