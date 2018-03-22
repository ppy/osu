// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Skinning
{
    public class LocalSkinOverrideContainer : Container, ISkinSource
    {
        public event Action SourceChanged;

        public Drawable GetDrawableComponent(string componentName) => source.GetDrawableComponent(componentName) ?? fallbackSource?.GetDrawableComponent(componentName);

        public Texture GetTexture(string componentName) => source.GetTexture(componentName) ?? fallbackSource.GetTexture(componentName);

        public SampleChannel GetSample(string sampleName) => source.GetSample(sampleName) ?? fallbackSource?.GetSample(sampleName);

        public TValue? GetValue<TConfiguration, TValue>(Func<TConfiguration, TValue?> query) where TConfiguration : SkinConfiguration where TValue : struct
        {
            TValue? val = null;
            var conf = (source as Skin)?.Configuration as TConfiguration;
            if (conf != null)
                val = query?.Invoke(conf);

            return val ?? fallbackSource?.GetValue(query);
        }

        public TValue GetValue<TConfiguration, TValue>(Func<TConfiguration, TValue> query) where TConfiguration : SkinConfiguration where TValue : class
        {
            TValue val = null;
            var conf = (source as Skin)?.Configuration as TConfiguration;
            if (conf != null)
                val = query?.Invoke(conf);

            return val ?? fallbackSource?.GetValue(query);
        }

        private readonly ISkinSource source;
        private ISkinSource fallbackSource;

        public LocalSkinOverrideContainer(ISkinSource source)
        {
            this.source = source;
        }

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateLocalDependencies(parent));

            fallbackSource = dependencies.Get<ISkinSource>();
            if (fallbackSource != null)
                fallbackSource.SourceChanged += () => SourceChanged?.Invoke();

            dependencies.CacheAs<ISkinSource>(this);

            return dependencies;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (fallbackSource != null)
                fallbackSource.SourceChanged -= SourceChanged;
        }
    }
}
