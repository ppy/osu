// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osuTK;

namespace osu.Game.Skinning
{
    public class SkinnableDrawable : SkinnableDrawable<Drawable>
    {
        public SkinnableDrawable(string name, Func<string, Drawable> defaultImplementation, Func<ISkinSource, bool> allowFallback = null, bool restrictSize = true)
            : base(name, defaultImplementation, allowFallback, restrictSize)
        {
        }
    }

    /// <summary>
    /// A drawable which can be skinned via an <see cref="ISkinSource"/>.
    /// </summary>
    /// <typeparam name="T">The type of drawable.</typeparam>
    public class SkinnableDrawable<T> : SkinReloadableDrawable
        where T : Drawable
    {
        /// <summary>
        /// The displayed component. May or may not be a type-<typeparamref name="T"/> member.
        /// </summary>
        protected Drawable Drawable { get; private set; }

        private readonly string componentName;

        private readonly bool restrictSize;

        /// <summary>
        /// Create a new skinnable drawable.
        /// </summary>
        /// <param name="name">The namespace-complete resource name for this skinnable element.</param>
        /// <param name="defaultImplementation">A function to create the default skin implementation of this element.</param>
        /// <param name="allowFallback">A conditional to decide whether to allow fallback to the default implementation if a skinned element is not present.</param>
        /// <param name="restrictSize">Whether a user-skin drawable should be limited to the size of our parent.</param>
        public SkinnableDrawable(string name, Func<string, T> defaultImplementation, Func<ISkinSource, bool> allowFallback = null, bool restrictSize = true)
            : this(name, allowFallback, restrictSize)
        {
            createDefault = defaultImplementation;
        }

        protected SkinnableDrawable(string name, Func<ISkinSource, bool> allowFallback = null, bool restrictSize = true)
            : base(allowFallback)
        {
            componentName = name;
            this.restrictSize = restrictSize;

            RelativeSizeAxes = Axes.Both;
        }

        private readonly Func<string, T> createDefault;

        protected virtual T CreateDefault(string name) => createDefault(name);

        /// <summary>
        /// Whether to apply size restrictions (specified via <see cref="restrictSize"/>) to the default implementation.
        /// </summary>
        protected virtual bool ApplySizeRestrictionsToDefault => false;

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            Drawable = skin.GetDrawableComponent(componentName);

            bool isDefault = false;

            if (Drawable == null && allowFallback)
            {
                Drawable = CreateDefault(componentName);
                isDefault = true;
            }

            if (Drawable != null)
            {
                if (restrictSize && (!isDefault || ApplySizeRestrictionsToDefault))
                {
                    Drawable.RelativeSizeAxes = Axes.Both;
                    Drawable.Size = Vector2.One;
                    Drawable.Scale = Vector2.One;
                    Drawable.FillMode = FillMode.Fit;
                }

                Drawable.Origin = Anchor.Centre;
                Drawable.Anchor = Anchor.Centre;

                InternalChild = Drawable;
            }
            else
                ClearInternal();
        }
    }
}
