// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Layout;
using osuTK;

namespace osu.Game.Graphics.Containers
{
    public partial class ShearAlignedDrawable : CompositeDrawable
    {
        private readonly Vector2 shear;
        private readonly LayoutValue layout = new LayoutValue(Invalidation.MiscGeometry);

        public ShearAlignedDrawable(Vector2 shear, Drawable drawable)
        {
            this.shear = shear;

            RelativeSizeAxes = drawable.RelativeSizeAxes;
            AutoSizeAxes = Axes.Both & ~drawable.RelativeSizeAxes;

            InternalChild = drawable;

            AddLayout(layout);
        }

        protected override void Update()
        {
            base.Update();

            if (!layout.IsValid)
            {
                updateLayout();
                layout.Validate();
            }
        }

        private void updateLayout()
        {
            float shearWidth = shear.X * Parent!.DrawHeight;
            float relativeY = Parent!.DrawHeight == 0 ? 0 : InternalChild.ToSpaceOfOtherDrawable(Vector2.Zero, Parent).Y / Parent!.DrawHeight;
            Padding = new MarginPadding { Right = shearWidth * relativeY };
        }
    }
}
