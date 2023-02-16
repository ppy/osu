// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Skinning
{
    public partial class SkinnableTargetContainer : SkinReloadableDrawable, ISkinnableTarget
    {
        private Container? content;

        public GlobalSkinComponentLookup.LookupType Target { get; }

        public IBindableList<ISkinnableDrawable> Components => components;

        private readonly BindableList<ISkinnableDrawable> components = new BindableList<ISkinnableDrawable>();

        public override bool IsPresent => base.IsPresent || Scheduler.HasPendingTasks; // ensure that components are loaded even if the target container is hidden (ie. due to user toggle).

        public bool ComponentsLoaded { get; private set; }

        private CancellationTokenSource? cancellationSource;

        public SkinnableTargetContainer(GlobalSkinComponentLookup.LookupType target)
        {
            Target = target;
        }

        public void Reload(SkinnableInfo[] skinnableInfo)
        {
            var drawables = new List<Drawable>();

            foreach (var i in skinnableInfo)
                drawables.Add(i.CreateInstance());

            Reload(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = drawables,
            });
        }

        public void Reload() => Reload(CurrentSkin.GetDrawableComponent(new GlobalSkinComponentLookup(Target)) as Container);

        public void Reload(Container? componentsContainer)
        {
            ClearInternal();
            components.Clear();
            ComponentsLoaded = false;

            if (componentsContainer == null)
                return;

            content = componentsContainer;

            cancellationSource?.Cancel();
            cancellationSource = null;

            if (content != null)
            {
                LoadComponentAsync(content, wrapper =>
                {
                    AddInternal(wrapper);
                    components.AddRange(wrapper.Children.OfType<ISkinnableDrawable>());
                    ComponentsLoaded = true;
                }, (cancellationSource = new CancellationTokenSource()).Token);
            }
            else
                ComponentsLoaded = true;
        }

        /// <inheritdoc cref="ISkinnableTarget"/>
        /// <exception cref="NotSupportedException">Thrown when attempting to add an element to a target which is not supported by the current skin.</exception>
        /// <exception cref="ArgumentException">Thrown if the provided instance is not a <see cref="Drawable"/>.</exception>
        public void Add(ISkinnableDrawable component)
        {
            if (content == null)
                throw new NotSupportedException("Attempting to add a new component to a target container which is not supported by the current skin.");

            if (!(component is Drawable drawable))
                throw new ArgumentException($"Provided argument must be of type {nameof(Drawable)}.", nameof(component));

            content.Add(drawable);
            components.Add(component);
        }

        /// <inheritdoc cref="ISkinnableTarget"/>
        /// <exception cref="NotSupportedException">Thrown when attempting to add an element to a target which is not supported by the current skin.</exception>
        /// <exception cref="ArgumentException">Thrown if the provided instance is not a <see cref="Drawable"/>.</exception>
        public void Remove(ISkinnableDrawable component)
        {
            if (content == null)
                throw new NotSupportedException("Attempting to remove a new component from a target container which is not supported by the current skin.");

            if (!(component is Drawable drawable))
                throw new ArgumentException($"Provided argument must be of type {nameof(Drawable)}.", nameof(component));

            content.Remove(drawable, true);
            components.Remove(component);
        }

        protected override void SkinChanged(ISkinSource skin)
        {
            base.SkinChanged(skin);

            Reload();
        }
    }
}
