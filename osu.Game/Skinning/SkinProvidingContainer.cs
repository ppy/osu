// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A container which adds a provided <see cref="ISkin"/> to the DI skin lookup hierarchy.
    /// </summary>
    /// <remarks>
    /// This container will expose an <see cref="ISkinSource"/> to its children.
    /// The source will first consider the skin provided via the constructor (if any), then fallback
    /// to any <see cref="ISkinSource"/> providers in the parent DI hierarchy.
    /// </remarks>
    public partial class SkinProvidingContainer : Container, ISkinSource
    {
        public event Action? SourceChanged;

        protected ISkinSource? ParentSource { get; private set; }

        /// <summary>
        /// Whether falling back to parent <see cref="ISkinSource"/>s is allowed in this container.
        /// </summary>
        protected virtual bool AllowFallingBackToParent => true;

        protected virtual bool AllowDrawableLookup(ISkinComponentLookup lookup) => true;

        protected virtual bool AllowTextureLookup(string componentName) => true;

        protected virtual bool AllowSampleLookup(ISampleInfo sampleInfo) => true;

        protected virtual bool AllowConfigurationLookup => true;

        protected virtual bool AllowColourLookup => true;

        private readonly object sourceSetLock = new object();

        /// <summary>
        /// A dictionary mapping each <see cref="ISkin"/> source to a wrapper which handles lookup allowances.
        /// </summary>
        private (ISkin skin, DisableableSkinSource wrapped)[] skinSources = Array.Empty<(ISkin skin, DisableableSkinSource wrapped)>();

        /// <summary>
        /// Constructs a new <see cref="SkinProvidingContainer"/> initialised with a single skin source.
        /// </summary>
        public SkinProvidingContainer(ISkin? skin)
            : this()
        {
            if (skin != null)
                SetSources(new[] { skin });
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

        public ISkin? FindProvider(Func<ISkin, bool> lookupFunction)
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

        public Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            foreach (var (_, lookupWrapper) in skinSources)
            {
                Drawable? sourceDrawable;
                if ((sourceDrawable = lookupWrapper.GetDrawableComponent(lookup)) != null)
                    return sourceDrawable;
            }

            if (!AllowFallingBackToParent)
                return null;

            return ParentSource?.GetDrawableComponent(lookup);
        }

        public SerialisedDrawableInfo? GetConfiguration(ISkinComponentLookup lookup)
        {
            foreach (var (_, lookupWrapper) in skinSources)
            {
                SerialisedDrawableInfo? sourceConfig;
                if ((sourceConfig = lookupWrapper.GetConfiguration(lookup)) != null)
                    return sourceConfig;
            }

            if (!AllowFallingBackToParent)
                return null;

            return ParentSource?.GetConfiguration(lookup);
        }

        public Texture? GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
        {
            foreach (var (_, lookupWrapper) in skinSources)
            {
                Texture? sourceTexture;
                if ((sourceTexture = lookupWrapper.GetTexture(componentName, wrapModeS, wrapModeT)) != null)
                    return sourceTexture;
            }

            if (!AllowFallingBackToParent)
                return null;

            return ParentSource?.GetTexture(componentName, wrapModeS, wrapModeT);
        }

        public ISample? GetSample(ISampleInfo sampleInfo)
        {
            foreach (var (_, lookupWrapper) in skinSources)
            {
                ISample? sourceSample;
                if ((sourceSample = lookupWrapper.GetSample(sampleInfo)) != null)
                    return sourceSample;
            }

            if (!AllowFallingBackToParent)
                return null;

            return ParentSource?.GetSample(sampleInfo);
        }

        public IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
            where TLookup : notnull
            where TValue : notnull
        {
            try
            {
                Skin.LogLookupDebug(this, lookup, Skin.LookupDebugType.Enter);

                foreach (var (_, lookupWrapper) in skinSources)
                {
                    IBindable<TValue>? bindable;
                    if ((bindable = lookupWrapper.GetConfig<TLookup, TValue>(lookup)) != null)
                        return bindable;
                }

                if (!AllowFallingBackToParent)
                    return null;

                return ParentSource?.GetConfig<TLookup, TValue>(lookup);
            }
            finally
            {
                Skin.LogLookupDebug(this, lookup, Skin.LookupDebugType.Exit);
            }
        }

        /// <summary>
        /// Replace the sources used for lookups in this container.
        /// </summary>
        /// <remarks>
        /// This does not implicitly fire a <see cref="SourceChanged"/> event. Consider calling <see cref="TriggerSourceChanged"/> if required.
        /// </remarks>
        /// <param name="sources">The new sources.</param>
        protected void SetSources(IEnumerable<ISkin> sources)
        {
            lock (sourceSetLock)
            {
                foreach (var skin in skinSources)
                {
                    if (skin.skin is ISkinSource source)
                        source.SourceChanged -= TriggerSourceChanged;
                }

                skinSources = sources.Select(skin => (skin, new DisableableSkinSource(skin, this))).ToArray();

                foreach (var skin in skinSources)
                {
                    if (skin.skin is ISkinSource source)
                        source.SourceChanged += TriggerSourceChanged;
                }
            }
        }

        /// <summary>
        /// Invoked after any consumed source change, before the external <see cref="SourceChanged"/> event is fired.
        /// This is also invoked once initially during <see cref="CreateChildDependencies"/> to ensure sources are ready for children consumption.
        /// </summary>
        protected virtual void RefreshSources() { }

        protected void TriggerSourceChanged()
        {
            // Expose to implementations, giving them a chance to react before notifying external consumers.
            RefreshSources();

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

            public Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
            {
                if (provider.AllowDrawableLookup(lookup))
                    return skin.GetDrawableComponent(lookup);

                return null;
            }

            public SerialisedDrawableInfo? GetConfiguration(ISkinComponentLookup lookup)
            {
                // Todo: Maybe wrong?
                if (provider.AllowDrawableLookup(lookup))
                    return skin.GetConfiguration(lookup);

                return null;
            }

            public Texture? GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
            {
                if (provider.AllowTextureLookup(componentName))
                    return skin.GetTexture(componentName, wrapModeS, wrapModeT);

                return null;
            }

            public ISample? GetSample(ISampleInfo sampleInfo)
            {
                if (provider.AllowSampleLookup(sampleInfo))
                    return skin.GetSample(sampleInfo);

                return null;
            }

            public IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
                where TLookup : notnull
                where TValue : notnull
            {
                try
                {
                    Skin.LogLookupDebug(this, lookup, Skin.LookupDebugType.Enter);

                    switch (lookup)
                    {
                        case GlobalSkinColours:
                        case SkinComboColourLookup:
                        case SkinCustomColourLookup:
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
                finally
                {
                    Skin.LogLookupDebug(this, lookup, Skin.LookupDebugType.Exit);
                }
            }

            public override string ToString() => $"{GetType().ReadableName()} {{ Skin: {skin} }}";
        }
    }
}
