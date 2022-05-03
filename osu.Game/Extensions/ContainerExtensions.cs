// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Extensions
{
    public static class ContainerExtensions
    {
        /// <summary>
        /// Keeps the transformations of this container's children the same as the transformations of this container's parent.
        /// </summary>
        /// <param name="target">The target to prevent keep its childrens' transformation.</param>
        /// <param name="parent">The parent of the target.</param>
        public static void KeepChildrenUpright(this Container target, Container parent)
        {
            float parentRotation = parent.Rotation;
            foreach (Drawable child in target)
            {
                child.RotateTo(-parentRotation);
            }
        }
    }
}
