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
    public class UprightUnstretchedContainer : Container
    {
        protected override Container<Drawable> Content => content;
        private readonly Container content;

        /// <summary>
        /// Controls how much this container scales compared to its parent (default is 1.0f).
        /// </summary>
        public float ScalingFactor { get; set; }

        /// <summary>
        /// Controls the scaling of this container.
        /// </summary>
        public ScaleMode Scaling { get; set; }

        public UprightUnstretchedContainer()
        {
            Scaling = ScaleMode.NoScaling;
            InternalChild = content = new GrowToFitContainer();
            AddLayout(layout);
        }

        private readonly LayoutValue layout = new LayoutValue(Invalidation.DrawInfo, InvalidationSource.Parent);

        protected override void Update()
        {
            base.Update();

            if (!layout.IsValid)
            {
                keepUprightAndUnstretched();
                layout.Validate();
            }
        }

        /// <summary>
        /// Keeps the drawable upright and unstretched preventing it from being rotated, sheared, scaled or flipped with its Parent.
        /// </summary>
        private void keepUprightAndUnstretched()
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

            // Extract shear.
            float alpha = reversedParrent.M21 / reversedParrent.M22;
            Shear = new Vector2(-alpha, 0);

            // Etract scale.
            float sx = reversedParrent.M11;
            float sy = reversedParrent.M22;

            Vector3 parentScale = parentMatrix.ExtractScale();

            float usedScale = 1.0f;

            switch (Scaling)
            {
                case ScaleMode.Horizontal:
                    usedScale = parentScale.X;
                    break;

                case ScaleMode.Vertical:
                    usedScale = parentScale.Y;
                    break;
            }

            usedScale = 1.0f + (usedScale - 1.0f) * ScalingFactor;

            Scale = new Vector2(sx * usedScale, sy * usedScale);
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

    public enum ScaleMode
    {
        /// <summary>
        /// Prevent this container from scaling.
        /// </summary>
        NoScaling,

        /// <summary>
        /// Scale This container (vertically and horizontally) with the vertical axis of its parent
        /// preserving the aspect ratio of the container.
        /// </summary>
        Vertical,

        /// <summary>
        /// Scales This container (vertically and horizontally) with the horizontal axis of its parent
        /// preserving the aspect ratio of the container.
        /// </summary>
        Horizontal,
    }
}
