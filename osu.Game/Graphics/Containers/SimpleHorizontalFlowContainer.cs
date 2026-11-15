// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Layout;

namespace osu.Game.Graphics.Containers
{
    public partial class SimpleHorizontalFlowContainer : SimpleHorizontalFlowContainer<Drawable>
    {
    }

    /// <summary>
    /// A container which lays out its children horizontally next to each other,
    /// but otherwise does not attempt to further impose any layout decisions.
    /// </summary>
    /// <remarks>
    /// Useful when you want to animate its children along the Y axis, where
    /// a standard <see cref="FillFlowContainer{T}"/> would have prevented
    /// any transforms from happening without having to resort to doing things
    /// like transforming a nested container.
    /// </remarks>
    public partial class SimpleHorizontalFlowContainer<T> : Container<T>
        where T : Drawable
    {
        public float Spacing { get; init; }

        private readonly LayoutValue childLayout = new LayoutValue(Invalidation.RequiredParentSizeToFit, InvalidationSource.Child);

        public SimpleHorizontalFlowContainer()
        {
            AddLayout(childLayout);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (!childLayout.IsValid)
            {
                performLayout();
                childLayout.Validate();
            }
        }

        private void performLayout()
        {
            float pos = 0;

            foreach (var child in Children)
            {
                child.X = pos;
                pos += child.DrawWidth + Spacing;
            }
        }
    }
}
