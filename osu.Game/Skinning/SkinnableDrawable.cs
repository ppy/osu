// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;

namespace osu.Game.Skinning
{
    public class SkinnableDrawable : SkinnableDrawable<Drawable>
    {
        public SkinnableDrawable(string name, Func<string, Drawable> defaultImplementation, bool fallback = true)
            : base(name, defaultImplementation, fallback)
        {
        }
    }

    public class SkinnableDrawable<T> : SkinReloadableDrawable
        where T : Drawable
    {
        private readonly Func<string, T> createDefault;

        private readonly string componentName;

        public SkinnableDrawable(string name, Func<string, T> defaultImplementation, bool fallback = true) : base(fallback)
        {
            componentName = name;
            createDefault = defaultImplementation;

            RelativeSizeAxes = Axes.Both;
        }

        protected override void SkinChanged(Skin skin, bool allowFallback)
        {
            var drawable = skin.GetDrawableComponent(componentName);
            if (drawable == null && allowFallback)
                drawable = createDefault(componentName);

            if (drawable != null)
                InternalChild = drawable;
            else
                ClearInternal();
        }
    }
}
