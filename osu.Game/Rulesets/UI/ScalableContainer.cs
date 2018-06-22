// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// A <see cref="Container"/> which can have its internal coordinate system scaled to a specific size.
    /// </summary>
    public class ScalableContainer : Container
    {
        /// <summary>
        /// A function that converts coordinates from gamefield to screen space.
        /// </summary>
        public Func<Vector2, Vector2> GamefieldToScreenSpace => scaledContent.GamefieldToScreenSpace;

        /// <summary>
        /// The scaled content.
        /// </summary>
        private readonly ScaledContainer scaledContent;
        protected override Container<Drawable> Content => scaledContent;

        /// <summary>
        /// A <see cref="Container"/> which can have its internal coordinate system scaled to a specific size.
        /// </summary>
        /// <param name="customWidth">The width to scale the internal coordinate space to.
        /// May be null if scaling based on <paramref name="customHeight"/> is desired. If <paramref name="customHeight"/> is also null, no scaling will occur.
        /// </param>
        /// <param name="customHeight">The height to scale the internal coordinate space to.
        /// May be null if scaling based on <paramref name="customWidth"/> is desired. If <paramref name="customWidth"/> is also null, no scaling will occur.
        /// </param>
        public ScalableContainer(float? customWidth = null, float? customHeight = null)
        {
            AddInternal(scaledContent = new ScaledContainer
            {
                CustomWidth = customWidth,
                CustomHeight = customHeight,
                RelativeSizeAxes = Axes.Both,
            });
        }

        private class ScaledContainer : Container
        {
            /// <summary>
            /// A function that converts coordinates from gamefield to screen space.
            /// </summary>
            public Func<Vector2, Vector2> GamefieldToScreenSpace => content.ToScreenSpace;

            /// <summary>
            /// The value to scale the width of the content to match.
            /// If null, <see cref="CustomHeight"/> is used.
            /// </summary>
            public float? CustomWidth;

            /// <summary>
            /// The value to scale the height of the content to match.
            /// if null, <see cref="CustomWidth"/> is used.
            /// </summary>
            public float? CustomHeight;

            private readonly Container content;
            protected override Container<Drawable> Content => content;

            public ScaledContainer()
            {
                AddInternal(content = new Container { RelativeSizeAxes = Axes.Both });
            }

            protected override void Update()
            {
                base.Update();

                content.Scale = sizeScale;
                content.Size = Vector2.Divide(Vector2.One, sizeScale);
            }

            /// <summary>
            /// The scale that is required for the size of the content to match <see cref="CustomWidth"/> and <see cref="CustomHeight"/>.
            /// </summary>
            private Vector2 sizeScale
            {
                get
                {
                    if (CustomWidth.HasValue && CustomHeight.HasValue)
                        return Vector2.Divide(DrawSize, new Vector2(CustomWidth.Value, CustomHeight.Value));
                    if (CustomWidth.HasValue)
                        return new Vector2(DrawSize.X / CustomWidth.Value);
                    if (CustomHeight.HasValue)
                        return new Vector2(DrawSize.Y / CustomHeight.Value);
                    return Vector2.One;
                }
            }
        }
    }
}
