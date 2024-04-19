// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A container which holds many skinnable components, with functionality to add, remove and reload layouts.
    /// Used to allow user customisation of skin layouts.
    /// </summary>
    /// <remarks>
    /// This is currently used as a means of serialising skin layouts to files.
    /// Currently, one json file in a skin will represent one <see cref="SkinnableContainer"/>, containing
    /// the output of <see cref="ISerialisableDrawableContainer.CreateSerialisedInfo"/>.
    /// </remarks>
    public partial class SkinnableContainer : SkinReloadableDrawable, ISerialisableDrawableContainer
    {
        private Container? content;

        public ISkinComponentLookup Lookup { get; }

        public IBindableList<ISerialisableDrawable> Components => components;

        private readonly BindableList<ISerialisableDrawable> components = new BindableList<ISerialisableDrawable>();

        public override bool IsPresent => base.IsPresent || Scheduler.HasPendingTasks; // ensure that components are loaded even if the target container is hidden (ie. due to user toggle).

        public bool ComponentsLoaded { get; private set; }

        private CancellationTokenSource? cancellationSource;

        public SkinnableContainer(SkinnableContainerLookup lookup)
        {
            Lookup = lookup;
        }

        public void Reload(SerialisedDrawableInfo[] skinnableInfo)
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

        public void Reload() => Reload(CurrentSkin.GetDrawableComponent(Lookup) as Container);

        public void Reload(Container? componentsContainer)
        {
            ClearInternal();
            components.Clear();
            ComponentsLoaded = false;

            content = componentsContainer ?? new Container
            {
                RelativeSizeAxes = Axes.Both
            };

            cancellationSource?.Cancel();
            cancellationSource = null;

            LoadComponentAsync(content, wrapper =>
            {
                AddInternal(wrapper);
                components.AddRange(wrapper.Children.OfType<ISerialisableDrawable>());
                ComponentsLoaded = true;
            }, (cancellationSource = new CancellationTokenSource()).Token);
        }

        /// <inheritdoc cref="ISerialisableDrawableContainer"/>
        /// <exception cref="NotSupportedException">Thrown when attempting to add an element to a target which is not supported by the current skin.</exception>
        /// <exception cref="ArgumentException">Thrown if the provided instance is not a <see cref="Drawable"/>.</exception>
        public void Add(ISerialisableDrawable component)
        {
            if (content == null)
                throw new NotSupportedException("Attempting to add a new component to a target container which is not supported by the current skin.");

            if (!(component is Drawable drawable))
                throw new ArgumentException($"Provided argument must be of type {nameof(Drawable)}.", nameof(component));

            content.Add(drawable);
            components.Add(component);
        }

        /// <inheritdoc cref="ISerialisableDrawableContainer"/>
        /// <exception cref="NotSupportedException">Thrown when attempting to add an element to a target which is not supported by the current skin.</exception>
        /// <exception cref="ArgumentException">Thrown if the provided instance is not a <see cref="Drawable"/>.</exception>
        public void Remove(ISerialisableDrawable component, bool disposeImmediately)
        {
            if (content == null)
                throw new NotSupportedException("Attempting to remove a new component from a target container which is not supported by the current skin.");

            if (!(component is Drawable drawable))
                throw new ArgumentException($"Provided argument must be of type {nameof(Drawable)}.", nameof(component));

            content.Remove(drawable, disposeImmediately);
            components.Remove(component);
        }

        protected override void SkinChanged(ISkinSource skin)
        {
            base.SkinChanged(skin);

            Reload();
        }
    }
}
