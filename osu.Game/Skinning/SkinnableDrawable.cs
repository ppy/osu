// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using OpenTK;

namespace osu.Game.Skinning
{
    public class SkinnableDrawable : SkinnableDrawable<Drawable>
    {
        public SkinnableDrawable(string name, Func<string, Drawable> defaultImplementation, Func<ISkinSource, bool> allowFallback = null, bool restrictSize = true)
            : base(name, defaultImplementation, allowFallback, restrictSize)
        {
        }
    }

    public class SkinnableDrawable<T> : SkinReloadableDrawable
        where T : Drawable
    {
        private readonly Func<string, T> createDefault;

        private readonly string componentName;

        private readonly bool restrictSize;

        /// <summary>
        ///
        /// </summary>
        /// <param name="name">The namespace-complete resource name for this skinnable element.</param>
        /// <param name="defaultImplementation">A function to create the default skin implementation of this element.</param>
        /// <param name="fallback">Whther to fallback to the default implementation when a custom skin is specified but not implementation is present.</param>
        /// <param name="restrictSize">Whether a user-skin drawable should be limited to the size of our parent.</param>
        public SkinnableDrawable(string name, Func<string, T> defaultImplementation, Func<ISkinSource, bool> allowFallback = null, bool restrictSize = true) : base(allowFallback)
        {
            componentName = name;
            createDefault = defaultImplementation;
            this.restrictSize = restrictSize;

            RelativeSizeAxes = Axes.Both;
        }

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            var drawable = skin.GetDrawableComponent(componentName);
            if (drawable != null)
            {
                if (restrictSize)
                {
                    drawable.RelativeSizeAxes = Axes.Both;
                    drawable.Size = Vector2.One;
                    drawable.Scale = Vector2.One;
                    drawable.FillMode = FillMode.Fit;
                }
            }
            else if (allowFallback)
                drawable = createDefault(componentName);

            if (drawable != null)
            {
                drawable.Origin = Anchor.Centre;
                drawable.Anchor = Anchor.Centre;

                InternalChild = drawable;
            }
            else
                ClearInternal();
        }
    }
}
