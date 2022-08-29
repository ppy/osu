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
    public class UprightAspectMaintainingContainer : Container
    {
        /// <summary>
        /// Controls how much this container scales compared to its parent (default is 1.0f).
        /// </summary>
        public float ScalingFactor { get; set; } = 1;

        /// <summary>
        /// Controls the scaling of this container.
        /// </summary>
        public ScaleMode Scaling { get; set; } = ScaleMode.Vertical;

        /// <summary>
        /// If this is true, all wrapper containers will be set to grow with their content
        /// and not shrink back, this is used to fix the position of children that change
        /// in size when using AutoSizeAxes.
        /// </summary>
        public bool GrowToFitContent
        {
            get => growToFitContent;
            set
            {
                if (growToFitContent != value)
                {
                    foreach (GrowToFitContainer c in Children)
                        c.GrowToFitContentUpdated = true;
                    growToFitContent = value;
                }
            }
        }

        private bool growToFitContent = true;

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

        public override void Add(Drawable drawable)
        {
            var wrapper = new GrowToFitContainer();
            wrapper.Wrap(drawable);
            wrapper.AutoSizeAxes = Axes.None;
            drawable.Origin = drawable.Anchor = Anchor.Centre;
            base.Add(wrapper);
        }

        /// <summary>
        /// Keeps the drawable upright and unstretched preventing it from being rotated, sheared, scaled or flipped with its Parent.
        /// </summary>
        private void keepUprightAndUnstretched()
        {
            // Decomposes the inverse of the parent DrawInfo.Matrix into rotation, shear and scale.
            var parentMatrix = Parent.DrawInfo.Matrix;

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

        public class GrowToFitContainer : Container
        {
            private readonly LayoutValue layout = new LayoutValue(Invalidation.RequiredParentSizeToFit, InvalidationSource.Child);

            private Vector2 maxChildSize;

            public bool GrowToFitContentUpdated { get; set; }

            public GrowToFitContainer()
            {
                AddLayout(layout);
            }

            protected override void Update()
            {
                UprightAspectMaintainingContainer parent = (UprightAspectMaintainingContainer)Parent;

                if (!layout.IsValid || GrowToFitContentUpdated)
                {
                    if ((Child.RelativeSizeAxes & Axes.X) != 0)
                        RelativeSizeAxes |= Axes.X;
                    else
                    {
                        if (parent.GrowToFitContent)
                            Width = Math.Max(Child.Width * Child.Scale.X, maxChildSize.X);
                        else
                            Width = Child.Width * Child.Scale.X;
                    }

                    if ((Child.RelativeSizeAxes & Axes.Y) != 0)
                        RelativeSizeAxes |= Axes.Y;
                    else
                    {
                        if (parent.GrowToFitContent)
                            Height = Math.Max(Child.Height * Child.Scale.Y, maxChildSize.Y);
                        else
                            Height = Child.Height * Child.Scale.Y;
                    }

                    // reset max_child_size or update it
                    if (!parent.GrowToFitContent)
                        maxChildSize = Child.Size;
                    else
                    {
                        maxChildSize.X = MathF.Max(maxChildSize.X, Child.Size.X);
                        maxChildSize.Y = MathF.Max(maxChildSize.Y, Child.Size.Y);
                    }

                    GrowToFitContentUpdated = false;
                    layout.Validate();
                }
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
        /// Scale uniformly (maintaining aspect ratio) based on the vertical scale of the parent.
        /// </summary>
        Vertical,

        /// <summary>
        /// Scale uniformly (maintaining aspect ratio) based on the horizontal scale of the parent.
        /// </summary>
        Horizontal,
    }
}
