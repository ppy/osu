// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Skinning
{
    public class SkinnableDrawable : SkinnableDrawable<Drawable>
    {
        public SkinnableDrawable(string name, Func<string,Drawable> defaultImplementation)
            : base(name, defaultImplementation)
        {
            RelativeSizeAxes = Axes.Both;
        }
    }

    public class SkinnableDrawable<T> : CompositeDrawable
        where T : Drawable
    {
        private Bindable<Skin> skin;
        protected Func<string,T> CreateDefault;

        public string ComponentName { get; set; }

        public SkinnableDrawable(string name, Func<string,T> defaultImplementation)
        {
            ComponentName = name;
            CreateDefault = defaultImplementation;
        }

        [BackgroundDependencyLoader]
        private void load(SkinManager skinManager)
        {
            skin = skinManager.CurrentSkin.GetBoundCopy();
            skin.ValueChanged += updateComponent;
            skin.TriggerChange();
        }

        private void updateComponent(Skin skin)
        {
            InternalChild = skin.GetDrawableComponent(ComponentName) ?? CreateDefault(Name);
        }
    }
}
