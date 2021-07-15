﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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

        [CanBeNull]
        protected ISkinSource ParentSource { get; private set; }

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
        /// A dictionary mapping each <see cref="ISkin"/> source to a wrapper which handles lookup allowances.
        /// </summary>
        private readonly List<(ISkin skin, DisableableSkinSource wrapped)> skinSources = new List<(ISkin, DisableableSkinSource)>();

        /// <summary>
        /// Constructs a new <see cref="SkinProvidingContainer"/> initialised with a single skin source.
        /// </summary>
        public SkinProvidingContainer([CanBeNull] ISkin skin)
            : this()
        {
            if (skin != null)
                AddSource(skin);
        }

        /// <summary>
        /// Constructs a new <see cref="SkinProvidingContainer"/> with no sources.
        /// </summary>
        protected SkinProvidingContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            ParentSource = dependencies.Get<ISkinSource>();
            if (ParentSource != null)
                ParentSource.SourceChanged += TriggerSourceChanged;

            dependencies.CacheAs<ISkinSource>(this);

            TriggerSourceChanged();

            return dependencies;
        }

        public ISkin FindProvider(Func<ISkin, bool> lookupFunction)
        {
            foreach (var (skin, lookupWrapper) in skinSources)
            {
                if (lookupFunction(lookupWrapper))
                    return skin;
            }

            if (!AllowFallingBackToParent)
                return null;

            return ParentSource?.FindProvider(lookupFunction);
        }

        public IEnumerable<ISkin> AllSources
        {
            get
            {
                foreach (var i in skinSources)
                    yield return i.skin;

                if (AllowFallingBackToParent && ParentSource != null)
                {
                    foreach (var skin in ParentSource.AllSources)
                        yield return skin;
                }
            }
        }

        public Drawable GetDrawableComponent(ISkinComponent component)
        {
            foreach (var (_, lookupWrapper) in skinSources)
            {
                Drawable sourceDrawable;
                if ((sourceDrawable = lookupWrapper.GetDrawableComponent(component)) != null)
                    return sourceDrawable;
            }

            if (!AllowFallingBackToParent)
                return null;

            return ParentSource?.GetDrawableComponent(component);
        }

        public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
        {
            foreach (var (_, lookupWrapper) in skinSources)
            {
                Texture sourceTexture;
                if ((sourceTexture = lookupWrapper.GetTexture(componentName, wrapModeS, wrapModeT)) != null)
                    return sourceTexture;
            }

            if (!AllowFallingBackToParent)
                return null;

            return ParentSource?.GetTexture(componentName, wrapModeS, wrapModeT);
        }

        public ISample GetSample(ISampleInfo sampleInfo)
        {
            foreach (var (_, lookupWrapper) in skinSources)
            {
                ISample sourceSample;
                if ((sourceSample = lookupWrapper.GetSample(sampleInfo)) != null)
                    return sourceSample;
            }

            if (!AllowFallingBackToParent)
                return null;

            return ParentSource?.GetSample(sampleInfo);
        }

        public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
        {
            foreach (var (_, lookupWrapper) in skinSources)
            {
                IBindable<TValue> bindable;
                if ((bindable = lookupWrapper.GetConfig<TLookup, TValue>(lookup)) != null)
                    return bindable;
            }

            if (!AllowFallingBackToParent)
                return null;

            return ParentSource?.GetConfig<TLookup, TValue>(lookup);
        }

        /// <summary>
        /// Add a new skin to this provider. Will be added to the end of the lookup order precedence.
        /// </summary>
        /// <param name="skin">The skin to add.</param>
        protected void AddSource(ISkin skin)
        {
            skinSources.Add((skin, new DisableableSkinSource(skin, this)));

            if (skin is ISkinSource source)
                source.SourceChanged += TriggerSourceChanged;
        }

        /// <summary>
        /// Remove a skin from this provider.
        /// </summary>
        /// <param name="skin">The skin to remove.</param>
        protected void RemoveSource(ISkin skin)
        {
            if (skinSources.RemoveAll(s => s.skin == skin) == 0)
                return;

            if (skin is ISkinSource source)
                source.SourceChanged -= TriggerSourceChanged;
        }

        /// <summary>
        /// Clears all skin sources.
        /// </summary>
        protected void ResetSources()
        {
            foreach (var i in skinSources.ToArray())
                RemoveSource(i.skin);
        }

        /// <summary>
        /// Invoked when any source has changed (either <see cref="ParentSource"/> or a source registered via <see cref="AddSource"/>).
        /// This is also invoked once initially during <see cref="CreateChildDependencies"/> to ensure sources are ready for children consumption.
        /// </summary>
        protected virtual void OnSourceChanged() { }

        protected void TriggerSourceChanged()
        {
            // Expose to implementations, giving them a chance to react before notifying external consumers.
            OnSourceChanged();

            SourceChanged?.Invoke();
        }

        protected override void Dispose(bool isDisposing)
        {
            // Must be done before base.Dispose()
            SourceChanged = null;

            base.Dispose(isDisposing);

            if (ParentSource != null)
                ParentSource.SourceChanged -= TriggerSourceChanged;

            foreach (var i in skinSources)
            {
                if (i.skin is ISkinSource source)
                    source.SourceChanged -= TriggerSourceChanged;
            }
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
