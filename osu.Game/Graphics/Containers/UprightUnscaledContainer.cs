// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Layout;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container that prevents itself and its children from getting rotated, scaled or flipped with its Parent.
    /// </summary>
    public class UprightUnscaledContainer : Container
    {
        public UprightUnscaledContainer()
        {
            AddLayout(layout);
        }

        private LayoutValue layout = new LayoutValue(Invalidation.DrawInfo, InvalidationSource.Parent);

        protected override void Update()
        {
            base.Update();
            if (!layout.IsValid)
            {
                Extensions.DrawableExtensions.KeepUprightAndUnscaled(this);
                layout.Validate();
            }
        }
    }
}
