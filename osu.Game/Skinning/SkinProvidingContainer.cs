// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
        /// Skins which should be exposed by this container, in order of lookup precedence.
        /// </summary>
        protected readonly BindableList<ISkin> SkinSources = new BindableList<ISkin>();

        /// <summary>
        /// A dictionary mapping each <see cref="ISkin"/> from the <see cref="SkinSources"/>
        /// to one that performs the "allow lookup" checks before proceeding with a lookup.
        /// </summary>
        private readonly Dictionary<ISkin, DisableableSkinSource> disableableSkinSources = new Dictionary<ISkin, DisableableSkinSource>();

        [CanBeNull]
        private ISkinSource fallbackSource;

        /// <summary>
        /// Whether falling back to parent <see cref="ISkinSource"/>s is allowed in this container.
        /// </summary>
        protected virtual bool AllowFallingBackToParent => true;

        protected virtual bool AllowDrawableLookup(ISkinComponent component) => true;

        protected virtual bool AllowTextureLookup(string componentName) => true;

        protected virtual bool AllowSampleLookup(ISampleInfo componentName) => true;

        protected virtual bool AllowConfigurationLookup => true;

        protected virtual bool AllowColourLookup => true;

        /// <summary>
        /// Constructs a new <see cref="SkinProvidingContainer"/> initialised with a single skin source.
        /// </summary>
        public SkinProvidingContainer([CanBeNull] ISkin skin)
            : this()
        {
            if (skin != null)
                SkinSources.Add(skin);
        }

        /// <summary>
        /// Constructs a new <see cref="SkinProvidingContainer"/> with no sources.
        /// Implementations can add or change sources through the <see cref="SkinSources"/> list.
        /// </summary>
        protected SkinProvidingContainer()
        {
            RelativeSizeAxes = Axes.Both;

            SkinSources.BindCollectionChanged(((_, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var skin in args.NewItems.Cast<ISkin>())
                        {
                            disableableSkinSources.Add(skin, new DisableableSkinSource(skin, this));

                            if (skin is ISkinSource source)
                                source.SourceChanged += OnSourceChanged;
                        }

                        break;

                    case NotifyCollectionChangedAction.Reset:
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var skin in args.OldItems.Cast<ISkin>())
                        {
                            disableableSkinSources.Remove(skin);

                            if (skin is ISkinSource source)
                                source.SourceChanged -= OnSourceChanged;
                        }

                        break;

                    case NotifyCollectionChangedAction.Replace:
                        foreach (var skin in args.OldItems.Cast<ISkin>())
                        {
                            disableableSkinSources.Remove(skin);

                            if (skin is ISkinSource source)
                                source.SourceChanged -= OnSourceChanged;
                        }

                        foreach (var skin in args.NewItems.Cast<ISkin>())
                        {
                            disableableSkinSources.Add(skin, new DisableableSkinSource(skin, this));

                            if (skin is ISkinSource source)
                                source.SourceChanged += OnSourceChanged;
                        }

                        break;
                }
            }), true);
        }

        public ISkin FindProvider(Func<ISkin, bool> lookupFunction)
        {
            foreach (var skin in SkinSources)
            {
                if (lookupFunction(disableableSkinSources[skin]))
                    return skin;
            }

            return fallbackSource?.FindProvider(lookupFunction);
        }

        public IEnumerable<ISkin> AllSources
        {
            get
            {
                foreach (var skin in SkinSources)
                    yield return skin;

                if (fallbackSource != null)
                {
                    foreach (var skin in fallbackSource.AllSources)
                        yield return skin;
                }
            }
        }

        public Drawable GetDrawableComponent(ISkinComponent component)
        {
            foreach (var skin in SkinSources)
            {
                Drawable sourceDrawable;
                if ((sourceDrawable = disableableSkinSources[skin]?.GetDrawableComponent(component)) != null)
                    return sourceDrawable;
            }

            return fallbackSource?.GetDrawableComponent(component);
        }

        public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
        {
            foreach (var skin in SkinSources)
            {
                Texture sourceTexture;
                if ((sourceTexture = disableableSkinSources[skin]?.GetTexture(componentName, wrapModeS, wrapModeT)) != null)
                    return sourceTexture;
            }

            return fallbackSource?.GetTexture(componentName, wrapModeS, wrapModeT);
        }

        public ISample GetSample(ISampleInfo sampleInfo)
        {
            foreach (var skin in SkinSources)
            {
                ISample sourceSample;
                if ((sourceSample = disableableSkinSources[skin]?.GetSample(sampleInfo)) != null)
                    return sourceSample;
            }

            return fallbackSource?.GetSample(sampleInfo);
        }

        public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
        {
            foreach (var skin in SkinSources)
            {
                IBindable<TValue> bindable;
                if ((bindable = disableableSkinSources[skin]?.GetConfig<TLookup, TValue>(lookup)) != null)
                    return bindable;
            }

            return fallbackSource?.GetConfig<TLookup, TValue>(lookup);
        }

        protected virtual void OnSourceChanged() => SourceChanged?.Invoke();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            if (AllowFallingBackToParent)
            {
                fallbackSource = dependencies.Get<ISkinSource>();
                if (fallbackSource != null)
                    fallbackSource.SourceChanged += OnSourceChanged;
            }

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

            foreach (var source in SkinSources.OfType<ISkinSource>())
                source.SourceChanged -= OnSourceChanged;
        }

        private class DisableableSkinSource : ISkin
        {
            private readonly ISkin skin;
            private readonly SkinProvidingContainer provider;

            public DisableableSkinSource(ISkin skin, SkinProvidingContainer provider)
            {
                this.skin = skin;
                this.provider = provider;
            }

            public Drawable GetDrawableComponent(ISkinComponent component)
            {
                if (provider.AllowDrawableLookup(component))
                    return skin.GetDrawableComponent(component);

                return null;
            }

            public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
            {
                if (provider.AllowTextureLookup(componentName))
                    return skin.GetTexture(componentName, wrapModeS, wrapModeT);

                return null;
            }

            public ISample GetSample(ISampleInfo sampleInfo)
            {
                if (provider.AllowSampleLookup(sampleInfo))
                    return skin.GetSample(sampleInfo);

                return null;
            }

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
            {
                switch (lookup)
                {
                    case GlobalSkinColours _:
                    case SkinCustomColourLookup _:
                        if (provider.AllowColourLookup)
                            return skin.GetConfig<TLookup, TValue>(lookup);

                        break;

                    default:
                        if (provider.AllowConfigurationLookup)
                            return skin.GetConfig<TLookup, TValue>(lookup);

                        break;
                }

                return null;
            }
        }
    }
}
