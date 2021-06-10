// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A container which adds a local <see cref="ISkinSource"/> to the hierarchy.
    /// </summary>
    public class SkinProvidingContainer : Container, ISkinSource
    {
        public event Action SourceChanged;

        /// <summary>
        /// The list of skins provided by this <see cref="SkinProvidingContainer"/>.
        /// </summary>
        protected readonly List<ISkin> SkinSources = new List<ISkin>();

        [CanBeNull]
        private ISkinSource fallbackSource;

        protected virtual bool AllowDrawableLookup(ISkinComponent component) => true;

        protected virtual bool AllowTextureLookup(string componentName) => true;

        protected virtual bool AllowSampleLookup(ISampleInfo componentName) => true;

        protected virtual bool AllowConfigurationLookup => true;

        protected virtual bool AllowColourLookup => true;

        /// <summary>
        /// Constructs a new <see cref="SkinProvidingContainer"/> with a single skin added to the protected <see cref="SkinSources"/> list.
        /// </summary>
        public SkinProvidingContainer(ISkin skin)
            : this()
        {
            SkinSources.Add(skin);
        }

        /// <summary>
        /// Constructs a new <see cref="SkinProvidingContainer"/> with no sources.
        /// Up to the implementation for adding to the <see cref="SkinSources"/> list.
        /// </summary>
        protected SkinProvidingContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public ISkin FindProvider(Func<ISkin, bool> lookupFunction)
        {
            foreach (var skin in SkinSources)
            {
                if (skin is ISkinSource source)
                {
                    if (source.FindProvider(lookupFunction) is ISkin found)
                        return found;
                }
                else if (skin != null)
                {
                    if (lookupFunction(skin))
                        return skin;
                }
            }

            return fallbackSource?.FindProvider(lookupFunction);
        }

        public Drawable GetDrawableComponent(ISkinComponent component)
        {
            if (AllowDrawableLookup(component))
            {
                foreach (var skin in SkinSources)
                {
                    Drawable sourceDrawable;
                    if ((sourceDrawable = skin?.GetDrawableComponent(component)) != null)
                        return sourceDrawable;
                }
            }

            return fallbackSource?.GetDrawableComponent(component);
        }

        public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
        {
            if (AllowTextureLookup(componentName))
            {
                foreach (var skin in SkinSources)
                {
                    Texture sourceTexture;
                    if ((sourceTexture = skin?.GetTexture(componentName, wrapModeS, wrapModeT)) != null)
                        return sourceTexture;
                }
            }

            return fallbackSource?.GetTexture(componentName, wrapModeS, wrapModeT);
        }

        public ISample GetSample(ISampleInfo sampleInfo)
        {
            if (AllowSampleLookup(sampleInfo))
            {
                foreach (var skin in SkinSources)
                {
                    ISample sourceSample;
                    if ((sourceSample = skin?.GetSample(sampleInfo)) != null)
                        return sourceSample;
                }
            }

            return fallbackSource?.GetSample(sampleInfo);
        }

        public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
        {
            if (lookup is GlobalSkinColours || lookup is SkinCustomColourLookup)
                return lookupWithFallback<TLookup, TValue>(lookup, AllowColourLookup);

            return lookupWithFallback<TLookup, TValue>(lookup, AllowConfigurationLookup);
        }

        private IBindable<TValue> lookupWithFallback<TLookup, TValue>(TLookup lookup, bool canUseSkinLookup)
        {
            if (canUseSkinLookup)
            {
                foreach (var skin in SkinSources)
                {
                    IBindable<TValue> bindable;
                    if ((bindable = skin?.GetConfig<TLookup, TValue>(lookup)) != null)
                        return bindable;
                }
            }

            return fallbackSource?.GetConfig<TLookup, TValue>(lookup);
        }

        protected virtual void OnSourceChanged() => SourceChanged?.Invoke();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            fallbackSource = dependencies.Get<ISkinSource>();
            if (fallbackSource != null)
                fallbackSource.SourceChanged += OnSourceChanged;

            dependencies.CacheAs<ISkinSource>(this);

            return dependencies;
        }

        protected override void Dispose(bool isDisposing)
        {
            // Must be done before base.Dispose()
            SourceChanged = null;

            base.Dispose(isDisposing);

            if (fallbackSource != null)
                fallbackSource.SourceChanged -= OnSourceChanged;
        }
    }
}
