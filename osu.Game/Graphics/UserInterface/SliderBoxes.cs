// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public partial class SliderBoxes : Container
    {
        public Box RightBox;
        public Box LeftBox;

        public SliderBoxes()
        {
            InternalChildren = new Drawable[]
            {
                RightBox = new Box
                {
                    EdgeSmoothness = new Vector2(0, 0.5f),
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                },
                LeftBox = new Box
                {
                    EdgeSmoothness = new Vector2(0, 0.5f),
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                }
            };
        }
    }
}
