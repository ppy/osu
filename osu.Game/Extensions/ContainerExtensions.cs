// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Extensions
{
    public static class ContainerExtensions
    {
        /// <summary>
        /// Prevents this container's children from transforming when the container transforms.
        /// </summary>
        /// <param name="target">The target to prevent its children from transforming.</param>
        /// <param name="parent">The parent of the target.</param>
        public static void KeepChildrenUpright(this Container target, Container parent)
        {
            float parentRotation = parent.Rotation;
            Vector2 parentScale = parent.Scale;

            foreach (Drawable child in target)
            {
                child.RotateTo(-parentRotation);

                // Checking rotations idea taken from https://github.com/ppy/osu/pull/15347/files
                int parentRotationInQuarterTurns = (int)Math.Floor(parentRotation / 90);
                if (parentRotationInQuarterTurns % 2 != 0)
                    child.ScaleTo(new Vector2(parentScale.Y, parentScale.X));
                else
                    child.ScaleTo(parentScale);
            }
        }
    }
}
