// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Layout;
using osuTK;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container that reverts any rotation (and optionally scale) applied by its direct parent.
    /// </summary>
    public partial class UprightAspectMaintainingContainer : Container
    {
        /// <summary>
        /// Controls how much this container scales compared to its parent (default is 1.0f).
        /// </summary>
        public float ScalingFactor { get; set; } = 1;

        /// <summary>
        /// Controls the scaling of this container.
        /// </summary>
        public ScaleMode Scaling { get; set; } = ScaleMode.Vertical;

        private readonly LayoutValue layout = new LayoutValue(Invalidation.DrawInfo, InvalidationSource.Parent);

        public UprightAspectMaintainingContainer()
        {
            AddLayout(layout);
        }

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
            // Decomposes the inverse of the parent DrawInfo.Matrix into rotation, shear and scale.
            var parentMatrix = Parent!.DrawInfo.Matrix;

            // Remove Translation.>
            parentMatrix.M31 = 0.0f;
            parentMatrix.M32 = 0.0f;

            Matrix3 reversedParent = parentMatrix.Inverted();

            // Extract the rotation.
            float angle = MathF.Atan2(reversedParent.M12, reversedParent.M11);
            Rotation = MathHelper.RadiansToDegrees(angle);

            // Remove rotation from the C matrix so that it only contains shear and scale.
            Matrix3 m = Matrix3.CreateRotationZ(-angle);
            reversedParent *= m;

            // Extract shear.
            float alpha = reversedParent.M21 / reversedParent.M22;
            Shear = new Vector2(-alpha, 0);

            // Etract scale.
            float sx = reversedParent.M11;
            float sy = reversedParent.M22;

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

            if (Scaling != ScaleMode.NoScaling)
            {
                if (ScalingFactor < 1.0f)
                    usedScale = 1.0f + (usedScale - 1.0f) * ScalingFactor;
                if (ScalingFactor > 1.0f)
                    usedScale = (usedScale < 1.0f) ? usedScale * (1.0f / ScalingFactor) : usedScale * ScalingFactor;
            }

            Scale = new Vector2(sx * usedScale, sy * usedScale);
        }
    }

    public enum ScaleMode
    {
        /// <summary>
        /// Prevent this container from scaling.
        /// </summary>
        NoScaling,

        /// <summary>
        /// Scale uniformly (maintaining aspect ratio) based on the vertical scale of the parent.
        /// </summary>
        Vertical,

        /// <summary>
        /// Scale uniformly (maintaining aspect ratio) based on the horizontal scale of the parent.
        /// </summary>
        Horizontal,
    }
}
