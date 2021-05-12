// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Extensions;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Skinning
{
    public class SkinnableElementTargetContainer : SkinReloadableDrawable, ISkinnableTarget
    {
        private SkinnableTargetWrapper content;

        public SkinnableTarget Target { get; }

        public IBindableList<ISkinnableComponent> Components => components;

        private readonly BindableList<ISkinnableComponent> components = new BindableList<ISkinnableComponent>();

        public SkinnableElementTargetContainer(SkinnableTarget target)
        {
            Target = target;
        }

        public void Reload()
        {
            ClearInternal();
            components.Clear();

            content = CurrentSkin.GetDrawableComponent(new SkinnableTargetComponent(Target)) as SkinnableTargetWrapper;

            if (content != null)
            {
                LoadComponentAsync(content, wrapper =>
                {
                    AddInternal(wrapper);
                    components.AddRange(wrapper.Children.OfType<ISkinnableComponent>());
                });
            }
        }

        public void Add(ISkinnableComponent component)
        {
            if (content == null)
                throw new NotSupportedException("Attempting to add a new component to a target container which is not supported by the current skin.");

            if (!(component is Drawable drawable))
                throw new ArgumentException("Provided argument must be of type {nameof(ISkinnableComponent)}.", nameof(drawable));

            content.Add(drawable);
            components.Add(component);
        }

        public IEnumerable<SkinnableInfo> CreateSerialisedChildren() =>
            components.Select(d => ((Drawable)d).CreateSerialisedInformation());

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);

            Reload();
        }
    }
}
