// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osuTK;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A drawable which can be skinned via an <see cref="ISkinSource"/>.
    /// </summary>
    public partial class SkinnableDrawable : SkinReloadableDrawable, ISerialisableDrawableContainer
    {
        /// <summary>
        /// The displayed component.
        /// </summary>
        public Drawable Drawable { get; private set; } = null!;

        /// <summary>
        /// Whether the drawable component should be centered in available space.
        /// Defaults to true.
        /// </summary>
        public bool CentreComponent = true;

        public new Axes AutoSizeAxes
        {
            get => base.AutoSizeAxes;
            set => base.AutoSizeAxes = value;
        }

        public IBindableList<ISerialisableDrawable> Components => components;

        public bool ComponentsLoaded { get; private set; }

        public ISkinComponentLookup Lookup { get; }

        private readonly BindableList<ISerialisableDrawable> components = new BindableList<ISerialisableDrawable>();
        private readonly ConfineMode confineMode;

        /// <summary>
        /// Create a new skinnable drawable.
        /// </summary>
        /// <param name="lookup">The namespace-complete resource name for this skinnable element.</param>
        /// <param name="defaultImplementation">A function to create the default skin implementation of this element.</param>
        /// <param name="confineMode">How (if at all) the <see cref="Drawable"/> should be resize to fit within our own bounds.</param>
        public SkinnableDrawable(ISkinComponentLookup lookup, Func<ISkinComponentLookup, Drawable>? defaultImplementation = null, ConfineMode confineMode = ConfineMode.NoScaling)
            : this(lookup, confineMode)
        {
            createDefault = defaultImplementation;
        }

        protected SkinnableDrawable(ISkinComponentLookup lookup, ConfineMode confineMode = ConfineMode.NoScaling)
        {
            Lookup = lookup;
            this.confineMode = confineMode;

            RelativeSizeAxes = Axes.Both;
        }

        /// <summary>
        /// Seeks to the 0-th frame if the content of this <see cref="SkinnableDrawable"/> is an <see cref="IFramedAnimation"/>.
        /// </summary>
        public void ResetAnimation() => (Drawable as IFramedAnimation)?.GotoFrame(0);

        private readonly Func<ISkinComponentLookup, Drawable>? createDefault;

        private readonly Cached scaling = new Cached();

        private bool isDefault;

        protected virtual Drawable CreateDefault(ISkinComponentLookup lookup) => createDefault?.Invoke(lookup) ?? Empty();

        /// <summary>
        /// Whether to apply size restrictions (specified via <see cref="confineMode"/>) to the default implementation.
        /// </summary>
        protected virtual bool ApplySizeRestrictionsToDefault => false;

        protected override void SkinChanged(ISkinSource skin) => Reload();

        protected override void Update()
        {
            base.Update();

            if (!scaling.IsValid)
            {
                try
                {
                    if (isDefault && !ApplySizeRestrictionsToDefault) return;

                    if (Drawable is ISerialisableDrawable) return;

                    switch (confineMode)
                    {
                        case ConfineMode.ScaleToFit:
                            Drawable.RelativeSizeAxes = Axes.Both;
                            Drawable.Size = Vector2.One;
                            Drawable.Scale = Vector2.One;
                            Drawable.FillMode = FillMode.Fit;
                            break;
                    }
                }
                finally
                {
                    scaling.Validate();
                }
            }
        }

        public void Reload() => reload(CurrentSkin.GetDrawableComponent(Lookup));

        private void reload(Drawable? newComponent)
        {
            components.Clear();

            if (newComponent == null)
            {
                Drawable = CreateDefault(Lookup);
                isDefault = true;
            }
            else
            {
                Drawable = newComponent;
                isDefault = false;
            }

            scaling.Invalidate();

            if (Drawable is ISerialisableDrawable serialisable)
                components.Add(serialisable);
            else
            {
                if (CentreComponent)
                {
                    Drawable.Origin = Anchor.Centre;
                    Drawable.Anchor = Anchor.Centre;
                }
            }

            InternalChild = Drawable;
            ComponentsLoaded = true;
        }

        public void Reload(SerialisedDrawableInfo[] skinnableInfo) => reload(skinnableInfo.FirstOrDefault()?.CreateInstance());

        public void Add(ISerialisableDrawable drawable) => throw new NotSupportedException();

        public void Remove(ISerialisableDrawable component, bool disposeImmediately) => throw new NotSupportedException();
        public bool IsEditable => (Drawable as ISerialisableDrawable)?.IsEditable == true;
    }

    public enum ConfineMode
    {
        /// <summary>
        /// Don't apply any scaling. This allows the user element to be of any size, exceeding specified bounds.
        /// </summary>
        NoScaling,
        ScaleToFit,
    }
}
