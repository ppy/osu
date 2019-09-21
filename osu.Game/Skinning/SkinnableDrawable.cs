// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osuTK;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A drawable which can be skinned via an <see cref="ISkinSource"/>.
    /// </summary>
    public class SkinnableDrawable : SkinReloadableDrawable
    {
        /// <summary>
        /// The displayed component.
        /// </summary>
        public Drawable Drawable { get; private set; }

        private readonly ISkinComponent component;

        private readonly ConfineMode confineMode;

        /// <summary>
        /// Create a new skinnable drawable.
        /// </summary>
        /// <param name="component">The namespace-complete resource name for this skinnable element.</param>
        /// <param name="defaultImplementation">A function to create the default skin implementation of this element.</param>
        /// <param name="allowFallback">A conditional to decide whether to allow fallback to the default implementation if a skinned element is not present.</param>
        /// <param name="confineMode">How (if at all) the <see cref="Drawable"/> should be resize to fit within our own bounds.</param>
        public SkinnableDrawable(ISkinComponent component, Func<ISkinComponent, Drawable> defaultImplementation, Func<ISkinSource, bool> allowFallback = null, ConfineMode confineMode = ConfineMode.ScaleDownToFit)
            : this(component, allowFallback, confineMode)
        {
            createDefault = defaultImplementation;
        }

        protected SkinnableDrawable(ISkinComponent component, Func<ISkinSource, bool> allowFallback = null, ConfineMode confineMode = ConfineMode.ScaleDownToFit)
            : base(allowFallback)
        {
            this.component = component;
            this.confineMode = confineMode;

            RelativeSizeAxes = Axes.Both;
        }

        private readonly Func<ISkinComponent, Drawable> createDefault;

        private readonly Cached scaling = new Cached();

        private bool isDefault;

        protected virtual Drawable CreateDefault(ISkinComponent component) => createDefault(component);

        /// <summary>
        /// Whether to apply size restrictions (specified via <see cref="confineMode"/>) to the default implementation.
        /// </summary>
        protected virtual bool ApplySizeRestrictionsToDefault => false;

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            Drawable = skin.GetDrawableComponent(component);

            isDefault = false;

            if (Drawable == null && allowFallback)
            {
                Drawable = CreateDefault(component);
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
                try
                {
                    if (Drawable == null || (isDefault && !ApplySizeRestrictionsToDefault)) return;

                    switch (confineMode)
                    {
                        case ConfineMode.NoScaling:
                            return;

                        case ConfineMode.ScaleDownToFit:
                            if (Drawable.DrawSize.X <= DrawSize.X && Drawable.DrawSize.Y <= DrawSize.Y)
                                return;

                            break;
                    }

                    Drawable.RelativeSizeAxes = Axes.Both;
                    Drawable.Size = Vector2.One;
                    Drawable.Scale = Vector2.One;
                    Drawable.FillMode = FillMode.Fit;
                }
                finally
                {
                    scaling.Validate();
                }
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
