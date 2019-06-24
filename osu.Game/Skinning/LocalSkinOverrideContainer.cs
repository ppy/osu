// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Game.Configuration;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A container which overrides existing skin options with beatmap-local values.
    /// </summary>
    public class LocalSkinOverrideContainer : Container, ISkinSource
    {
        public event Action SourceChanged;

        private readonly Bindable<bool> beatmapSkins = new Bindable<bool>();
        private readonly Bindable<bool> beatmapHitsounds = new Bindable<bool>();

        private readonly ISkin skin;
        private ISkinSource fallbackSource;

        public LocalSkinOverrideContainer(ISkin skin)
        {
            this.skin = skin;
        }

        public Drawable GetDrawableComponent(string componentName)
        {
            Drawable sourceDrawable;
            if (beatmapSkins.Value && (sourceDrawable = skin.GetDrawableComponent(componentName)) != null)
                return sourceDrawable;

            return fallbackSource?.GetDrawableComponent(componentName);
        }

        public Texture GetTexture(string componentName)
        {
            Texture sourceTexture;
            if (beatmapSkins.Value && (sourceTexture = skin.GetTexture(componentName)) != null)
                return sourceTexture;

            return fallbackSource.GetTexture(componentName);
        }

        public SampleChannel GetSample(string sampleName)
        {
            SampleChannel sourceChannel;
            if (beatmapHitsounds.Value && (sourceChannel = skin.GetSample(sampleName)) != null)
                return sourceChannel;

            return fallbackSource?.GetSample(sampleName);
        }

        public TValue GetValue<TConfiguration, TValue>(Func<TConfiguration, TValue> query) where TConfiguration : SkinConfiguration
        {
            TValue val;
            if ((skin as Skin)?.Configuration is TConfiguration conf)
                if (beatmapSkins.Value && (val = query.Invoke(conf)) != null)
                    return val;

            return fallbackSource == null ? default : fallbackSource.GetValue(query);
        }

        private void onSourceChanged() => SourceChanged?.Invoke();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            fallbackSource = dependencies.Get<ISkinSource>();
            if (fallbackSource != null)
                fallbackSource.SourceChanged += onSourceChanged;

            dependencies.CacheAs<ISkinSource>(this);

            var config = dependencies.Get<OsuConfigManager>();

            config.BindWith(OsuSetting.BeatmapSkins, beatmapSkins);
            config.BindWith(OsuSetting.BeatmapHitsounds, beatmapHitsounds);

            beatmapSkins.BindValueChanged(_ => onSourceChanged());
            beatmapHitsounds.BindValueChanged(_ => onSourceChanged());

            return dependencies;
        }

        protected override void Dispose(bool isDisposing)
        {
            // Must be done before base.Dispose()
            SourceChanged = null;

            base.Dispose(isDisposing);

            if (fallbackSource != null)
                fallbackSource.SourceChanged -= onSourceChanged;
        }
    }
}
