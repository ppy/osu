// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Menu
{
    /// <summary>
    /// A flow container with an origin based on one of its contained drawables.
    /// </summary>
    public partial class FlowContainerWithOrigin : FillFlowContainer
    {
        /// <summary>
        /// A target drawable which this flowcontainer should be centered around.
        /// This target should be a direct child of this FlowContainer.
        /// </summary>
        public Drawable CentreTarget;

        protected override int Compare(Drawable x, Drawable y) => CompareReverseChildID(x, y);

        public override Anchor Origin => Anchor.Custom;

        public override Vector2 OriginPosition
        {
            get
            {
                if (CentreTarget == null)
                    return base.OriginPosition;

                return CentreTarget.DrawPosition + CentreTarget.DrawSize / 2 * CentreTarget.Scale;
            }
        }
    }
}
