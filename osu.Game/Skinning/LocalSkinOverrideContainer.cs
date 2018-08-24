// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Game.Configuration;

namespace osu.Game.Skinning
{
    public class LocalSkinOverrideContainer : Container, ISkinSource
    {
        public event Action SourceChanged;

        private Bindable<bool> beatmapSkins = new Bindable<bool>();
        private Bindable<bool> beatmapHitsounds = new Bindable<bool>();

        public Drawable GetDrawableComponent(string componentName)
        {
            Drawable sourceDrawable;
            if (beatmapSkins && (sourceDrawable = source.GetDrawableComponent(componentName)) != null)
                return sourceDrawable;
            return fallbackSource?.GetDrawableComponent(componentName);
        }

        public Texture GetTexture(string componentName)
        {
            Texture sourceTexture;
            if (beatmapSkins && (sourceTexture = source.GetTexture(componentName)) != null)
                return sourceTexture;
            return fallbackSource.GetTexture(componentName);
        }

        public SampleChannel GetSample(string sampleName)
        {
            SampleChannel sourceChannel;
            if (beatmapHitsounds && (sourceChannel = source.GetSample(sampleName)) != null)
                return sourceChannel;
            return fallbackSource?.GetSample(sampleName);
        }

        public TValue? GetValue<TConfiguration, TValue>(Func<TConfiguration, TValue?> query) where TConfiguration : SkinConfiguration where TValue : struct
        {
            TValue? val = null;
            if ((source as Skin)?.Configuration is TConfiguration conf)
                val = query?.Invoke(conf);

            return val ?? fallbackSource?.GetValue(query);
        }

        public TValue GetValue<TConfiguration, TValue>(Func<TConfiguration, TValue> query) where TConfiguration : SkinConfiguration where TValue : class
        {
            TValue val = null;
            if ((source as Skin)?.Configuration is TConfiguration conf)
                val = query?.Invoke(conf);

            return val ?? fallbackSource?.GetValue(query);
        }

        private readonly ISkinSource source;
        private ISkinSource fallbackSource;

        public LocalSkinOverrideContainer(ISkinSource source)
        {
            this.source = source;
        }

        private void onSourceChanged() => SourceChanged?.Invoke();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            fallbackSource = dependencies.Get<ISkinSource>();
            dependencies.CacheAs<ISkinSource>(this);

            return dependencies;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            beatmapSkins = config.GetBindable<bool>(OsuSetting.BeatmapSkins);
            beatmapSkins.BindValueChanged(_ => onSourceChanged());

            beatmapHitsounds = config.GetBindable<bool>(OsuSetting.BeatmapHitsounds);
            beatmapHitsounds.BindValueChanged(_ => onSourceChanged(), true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (fallbackSource != null)
                fallbackSource.SourceChanged += onSourceChanged;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (fallbackSource != null)
                fallbackSource.SourceChanged -= onSourceChanged;
        }
    }
}
