// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osuTK;

namespace osu.Game.Skinning
{
    public class SkinnableDrawable : SkinnableDrawable<Drawable>
    {
        public SkinnableDrawable(string name, Func<string, Drawable> defaultImplementation, Func<ISkinSource, bool> allowFallback = null, ConfineMode confineMode = ConfineMode.ScaleDownToFit)
            : base(name, defaultImplementation, allowFallback, confineMode)
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

        private readonly ConfineMode confineMode;

        /// <summary>
        /// Create a new skinnable drawable.
        /// </summary>
        /// <param name="name">The namespace-complete resource name for this skinnable element.</param>
        /// <param name="defaultImplementation">A function to create the default skin implementation of this element.</param>
        /// <param name="allowFallback">A conditional to decide whether to allow fallback to the default implementation if a skinned element is not present.</param>
        /// <param name="confineMode">How (if at all) the <see cref="Drawable"/> should be resize to fit within our own bounds.</param>
        public SkinnableDrawable(string name, Func<string, T> defaultImplementation, Func<ISkinSource, bool> allowFallback = null, ConfineMode confineMode = ConfineMode.ScaleDownToFit)
            : this(name, allowFallback, confineMode)
        {
            createDefault = defaultImplementation;
        }

        protected SkinnableDrawable(string name, Func<ISkinSource, bool> allowFallback = null, ConfineMode confineMode = ConfineMode.ScaleDownToFit)
            : base(allowFallback)
        {
            componentName = name;
            this.confineMode = confineMode;

            RelativeSizeAxes = Axes.Both;
        }

        private readonly Func<string, T> createDefault;

        private readonly Cached scaling = new Cached();

        private bool isDefault;

        protected virtual T CreateDefault(string name) => createDefault(name);

        /// <summary>
        /// Whether to apply size restrictions (specified via <see cref="confineMode"/>) to the default implementation.
        /// </summary>
        protected virtual bool ApplySizeRestrictionsToDefault => false;

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            Drawable = skin.GetDrawableComponent(componentName);

            isDefault = false;

            if (Drawable == null && allowFallback)
            {
                Drawable = CreateDefault(componentName);
                isDefault = true;
            }

            if (Drawable != null)
            {
                scaling.Invalidate();
                Drawable.Origin = Anchor.Centre;
                Drawable.Anchor = Anchor.Centre;

                InternalChild = Drawable;
            }
            else
                ClearInternal();
        }

        protected override void Update()
        {
            base.Update();

            if (!scaling.IsValid)
            {
                if (Drawable != null && confineMode != ConfineMode.NoScaling && (!isDefault || ApplySizeRestrictionsToDefault))
                {
                    bool applyScaling = confineMode == ConfineMode.ScaleToFit ||
                                        (confineMode == ConfineMode.ScaleDownToFit && (Drawable.DrawSize.X > DrawSize.X || Drawable.DrawSize.Y > DrawSize.Y));

                    if (applyScaling)
                    {
                        Drawable.RelativeSizeAxes = Axes.Both;
                        Drawable.Size = Vector2.One;
                        Drawable.Scale = Vector2.One;
                        Drawable.FillMode = FillMode.Fit;
                    }
                }

                scaling.Validate();
            }
        }
    }

    public enum ConfineMode
    {
        /// <summary>
        /// Don't apply any scaling. This allows the user element to be of any size, exceeding specified bounds.
        /// </summary>
        NoScaling,
        ScaleDownToFit,
        ScaleToFit,
    }
}
