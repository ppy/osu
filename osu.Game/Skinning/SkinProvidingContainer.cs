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

        private readonly NoFallbackProxy noFallbackLookupProxy;

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

            noFallbackLookupProxy = new NoFallbackProxy(this);

            if (skin is ISkinSource source)
                source.SourceChanged += TriggerSourceChanged;
        }

        public ISkin FindProvider(Func<ISkin, bool> lookupFunction)
        {
            foreach (var skin in SkinSources)
            {
                // a proxy must be used here to correctly pass through the "Allow" checks without implicitly falling back to the fallbackSource.
                if (lookupFunction(noFallbackLookupProxy))
                    return skin;
            }

            return fallbackSource?.FindProvider(lookupFunction);
        }

        public Drawable GetDrawableComponent(ISkinComponent component)
            => GetDrawableComponent(component, true);

        public Drawable GetDrawableComponent(ISkinComponent component, bool fallback)
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

            if (!fallback)
                return null;

            return fallbackSource?.GetDrawableComponent(component);
        }

        public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
            => GetTexture(componentName, wrapModeS, wrapModeT, true);

        public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT, bool fallback)
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

            if (!fallback)
                return null;

            return fallbackSource?.GetTexture(componentName, wrapModeS, wrapModeT);
        }

        public ISample GetSample(ISampleInfo sampleInfo)
            => GetSample(sampleInfo, true);

        public ISample GetSample(ISampleInfo sampleInfo, bool fallback)
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

            if (!fallback)
                return null;

            return fallbackSource?.GetSample(sampleInfo);
        }

        public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
            => GetConfig<TLookup, TValue>(lookup, true);

        public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup, bool fallback)
        {
            if (lookup is GlobalSkinColours || lookup is SkinCustomColourLookup)
                return lookupWithFallback<TLookup, TValue>(lookup, AllowColourLookup);

            return lookupWithFallback<TLookup, TValue>(lookup, AllowConfigurationLookup);
            if (skin != null)
            {
                if (lookup is GlobalSkinColours || lookup is SkinCustomColourLookup)
                    return lookupWithFallback<TLookup, TValue>(lookup, AllowColourLookup, fallback);

                return lookupWithFallback<TLookup, TValue>(lookup, AllowConfigurationLookup, fallback);
            }

            if (!fallback)
                return null;

            return fallbackSource?.GetConfig<TLookup, TValue>(lookup);
        }

        private IBindable<TValue> lookupWithFallback<TLookup, TValue>(TLookup lookup, bool canUseSkinLookup, bool canUseFallback)
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

            if (!canUseFallback)
                return null;

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
                fallbackSource.SourceChanged -= TriggerSourceChanged;

            if (skin is ISkinSource source)
                source.SourceChanged -= TriggerSourceChanged;
        }

        private class NoFallbackProxy : ISkinSource
        {
            private readonly SkinProvidingContainer provider;

            public NoFallbackProxy(SkinProvidingContainer provider)
            {
                this.provider = provider;
            }

            public Drawable GetDrawableComponent(ISkinComponent component)
                => provider.GetDrawableComponent(component, false);

            public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
                => provider.GetTexture(componentName, wrapModeS, wrapModeT, false);

            public ISample GetSample(ISampleInfo sampleInfo)
                => provider.GetSample(sampleInfo, false);

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
                => provider.GetConfig<TLookup, TValue>(lookup, false);

            public event Action SourceChanged
            {
                add => provider.SourceChanged += value;
                remove => provider.SourceChanged -= value;
            }

            public ISkin FindProvider(Func<ISkin, bool> lookupFunction) =>
                provider.FindProvider(lookupFunction);
        }
    }
}
