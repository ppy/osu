// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Layout;
using osuTK;
using System;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container that prevents itself and its children from getting rotated, scaled or flipped with its Parent.
    /// </summary>
    public class UprightUnscaledContainer : Container
    {
        protected override Container<Drawable> Content => content;
        private readonly Container content;

        public UprightUnscaledContainer()
        {
            InternalChild = content = new GrowToFitContainer();
            AddLayout(layout);
        }

        private readonly LayoutValue layout = new LayoutValue(Invalidation.DrawInfo, InvalidationSource.Parent);

        protected override void Update()
        {
            base.Update();

            if (!layout.IsValid)
            {
                keepUprightAndUnscaled();
                layout.Validate();
            }
        }

        /// <summary>
        /// Keeps the drawable upright and unstretched preventing it from being rotated, sheared, scaled or flipped with its Parent.
        /// </summary>
        private void keepUprightAndUnscaled()
        {
            // Decomposes the inverse of the parent FrawInfo.Matrix into rotation, shear and scale.
            var parentMatrix = Parent.DrawInfo.Matrix;

            // Remove Translation.
            parentMatrix.M31 = 0.0f;
            parentMatrix.M32 = 0.0f;

            Matrix3 reversedParrent = parentMatrix.Inverted();

            // Extract the rotation.
            float angle = MathF.Atan2(reversedParrent.M12, reversedParrent.M11);
            Rotation = MathHelper.RadiansToDegrees(angle);

            // Remove rotation from the C matrix so that it only contains shear and scale.
            Matrix3 m = Matrix3.CreateRotationZ(-angle);
            reversedParrent *= m;

            // Extract shear and scale.
            float sx = reversedParrent.M11;
            float sy = reversedParrent.M22;
            float alpha = reversedParrent.M21 / reversedParrent.M22;

            Scale = new Vector2(sx, sy);
            Shear = new Vector2(-alpha, 0);
        }

        /// <summary>
        /// A container that grows in size to fit its children and retains its size when its children shrink
        /// </summary>
        private class GrowToFitContainer : Container
        {
            protected override Container<Drawable> Content => content;
            private readonly Container content;

            public GrowToFitContainer()
            {
                InternalChild = content = new Container
                {
                    AutoSizeAxes = Axes.Both,
                };
            }

            protected override void Update()
            {
                base.Update();
                Height = Math.Max(content.Height, Height);
                Width = Math.Max(content.Width, Width);
            }
        }
    }
}
