// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Audio;
using osu.Game.Configuration;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A container which overrides existing skin options with beatmap-local values.
    /// </summary>
    public class BeatmapSkinProvidingContainer : SkinProvidingContainer
    {
        private readonly Bindable<bool> beatmapSkins = new Bindable<bool>();
        private readonly Bindable<bool> beatmapHitsounds = new Bindable<bool>();

        protected override bool AllowConfigurationLookup => beatmapSkins.Value;
        protected override bool AllowDrawableLookup(ISkinComponent component) => beatmapSkins.Value;
        protected override bool AllowTextureLookup(string componentName) => beatmapSkins.Value;
        protected override bool AllowSampleLookup(ISampleInfo componentName) => beatmapHitsounds.Value;

        public BeatmapSkinProvidingContainer(ISkin skin)
            : base(skin)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.BeatmapSkins, beatmapSkins);
            config.BindWith(OsuSetting.BeatmapHitsounds, beatmapHitsounds);

            beatmapSkins.BindValueChanged(_ => TriggerSourceChanged());
            beatmapHitsounds.BindValueChanged(_ => TriggerSourceChanged());
        }
    }
}
