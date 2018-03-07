// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using OpenTK;

namespace osu.Game.Skinning
{
    public class SkinnableDrawable : SkinnableDrawable<Drawable>
    {
        public SkinnableDrawable(string name, Func<string, Drawable> defaultImplementation, bool fallback = true, bool restrictSize = true)
            : base(name, defaultImplementation, fallback, restrictSize)
        {
        }
    }

    public class SkinnableDrawable<T> : SkinReloadableDrawable
        where T : Drawable
    {
        private readonly Func<string, T> createDefault;

        private readonly string componentName;

        /// <summary>
        /// Whether a user-skin drawable should be limited to the size of our parent.
        /// </summary>
        public readonly bool RestrictSize;

        public SkinnableDrawable(string name, Func<string, T> defaultImplementation, bool fallback = true, bool restrictSize = true) : base(fallback)
        {
            componentName = name;
            createDefault = defaultImplementation;
            RestrictSize = restrictSize;

            RelativeSizeAxes = Axes.Both;
        }

        protected override void SkinChanged(Skin skin, bool allowFallback)
        {
            var drawable = skin.GetDrawableComponent(componentName);
            if (drawable != null)
            {
                if (RestrictSize)
                {
                    drawable.RelativeSizeAxes = Axes.Both;
                    drawable.Size = Vector2.One;
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
