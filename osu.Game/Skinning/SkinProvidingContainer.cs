// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A container which adds a local <see cref="ISkinSource"/> to the hierarchy.
    /// </summary>
    public class SkinProvidingContainer : Container, ISkinSource
    {
        public event Action SourceChanged;

        private readonly ISkin skin;

        private ISkinSource fallbackSource;

        protected virtual bool AllowDrawableLookup(string componentName) => true;

        protected virtual bool AllowTextureLookup(string componentName) => true;

        protected virtual bool AllowSampleLookup(string componentName) => true;

        protected virtual bool AllowConfigurationLookup => true;

        public SkinProvidingContainer(ISkin skin)
        {
            this.skin = skin;
        }

        public Drawable GetDrawableComponent(string componentName)
        {
            Drawable sourceDrawable;
            if (AllowDrawableLookup(componentName) && (sourceDrawable = skin?.GetDrawableComponent(componentName)) != null)
                return sourceDrawable;

            return fallbackSource?.GetDrawableComponent(componentName);
        }

        public Texture GetTexture(string componentName)
        {
            Texture sourceTexture;
            if (AllowTextureLookup(componentName) && (sourceTexture = skin?.GetTexture(componentName)) != null)
                return sourceTexture;

            return fallbackSource.GetTexture(componentName);
        }

        public SampleChannel GetSample(string sampleName)
        {
            SampleChannel sourceChannel;
            if (AllowSampleLookup(sampleName) && (sourceChannel = skin?.GetSample(sampleName)) != null)
                return sourceChannel;

            return fallbackSource?.GetSample(sampleName);
        }

        public TValue GetValue<TConfiguration, TValue>(Func<TConfiguration, TValue> query) where TConfiguration : SkinConfiguration
        {
            TValue val;
            if (AllowConfigurationLookup && skin != null && (val = skin.GetValue(query)) != null)
                return val;

            return fallbackSource == null ? default : fallbackSource.GetValue(query);
        }

        protected virtual void TriggerSourceChanged() => SourceChanged?.Invoke();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            fallbackSource = dependencies.Get<ISkinSource>();
            if (fallbackSource != null)
                fallbackSource.SourceChanged += TriggerSourceChanged;

            dependencies.CacheAs<ISkinSource>(this);

            return dependencies;
        }

        protected override void Dispose(bool isDisposing)
        {
            // Must be done before base.Dispose()
            SourceChanged = null;

            base.Dispose(isDisposing);

            if (fallbackSource != null)
                fallbackSource.SourceChanged -= TriggerSourceChanged;
        }
    }
}
