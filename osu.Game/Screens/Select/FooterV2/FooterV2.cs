// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;

namespace osu.Game.Screens.Select.FooterV2
{
    public partial class FooterV2 : CompositeDrawable
    {
        //Should be 60, setting to 50 for now for the sake of matching the current BackButton height.
        private const int height = 50;

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            RelativeSizeAxes = Axes.X;
            Height = height;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colour.B5
                }
            };
        }

        protected override bool OnMouseDown(MouseDownEvent e) => true;

        protected override bool OnClick(ClickEvent e) => true;

        protected override bool OnHover(HoverEvent e) => true;
    }
}
