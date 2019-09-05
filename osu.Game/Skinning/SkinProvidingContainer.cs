// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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

        private readonly ISkin skin;

        private ISkinSource fallbackSource;

        protected virtual bool AllowDrawableLookup(ISkinComponent component) => true;

        protected virtual bool AllowTextureLookup(string componentName) => true;

        protected virtual bool AllowSampleLookup(ISampleInfo componentName) => true;

        protected virtual bool AllowConfigurationLookup => true;

        public SkinProvidingContainer(ISkin skin)
        {
            this.skin = skin;

            RelativeSizeAxes = Axes.Both;
        }

        public Drawable GetDrawableComponent(ISkinComponent component)
        {
            Drawable sourceDrawable;
            if (AllowDrawableLookup(component) && (sourceDrawable = skin?.GetDrawableComponent(component)) != null)
                return sourceDrawable;

            return fallbackSource?.GetDrawableComponent(component);
        }

        public Texture GetTexture(string componentName)
        {
            Texture sourceTexture;
            if (AllowTextureLookup(componentName) && (sourceTexture = skin?.GetTexture(componentName)) != null)
                return sourceTexture;

            return fallbackSource.GetTexture(componentName);
        }

        public SampleChannel GetSample(ISampleInfo sampleInfo)
        {
            SampleChannel sourceChannel;
            if (AllowSampleLookup(sampleInfo) && (sourceChannel = skin?.GetSample(sampleInfo)) != null)
                return sourceChannel;

            return fallbackSource?.GetSample(sampleInfo);
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
