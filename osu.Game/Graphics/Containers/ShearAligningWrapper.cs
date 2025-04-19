// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Layout;
using osuTK;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// Adds left padding based on direct parent to make sheared pieces in a vertical flow aligned appropriately.
    /// </summary>
    /// <remarks>
    /// See associated test scene for further demonstration.
    /// </remarks>
    public partial class ShearAligningWrapper : CompositeDrawable
    {
        private readonly LayoutValue layout = new LayoutValue(Invalidation.MiscGeometry);

        // the wrapped drawable may go off-screen as part of a transition, if it does so then this wrapper class becomes masked away as well
        // and the wrapped drawable cannot transform itself back to screen anymore. work around by disabling IsMaskedAway in the wrapper class.
        // this issue can be seen with BeatmapTitleWedge/BeatmapDetailsArea being unable to return back to screen after hiding from transition to gameplay.
        protected override bool ComputeIsMaskedAway(RectangleF maskingBounds) => false;

        public ShearAligningWrapper(Drawable drawable)
        {
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
            float shearWidth = OsuGame.SHEAR.X * Parent!.DrawHeight;
            float relativeY = Parent!.DrawHeight == 0 ? 0 : InternalChild.ToSpaceOfOtherDrawable(Vector2.Zero, Parent).Y / Parent!.DrawHeight;
            Padding = new MarginPadding { Left = shearWidth * relativeY };
        }
    }
}
