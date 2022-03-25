// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Storyboards;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A container which overrides existing skin options with beatmap-local values.
    /// </summary>
    public class BeatmapSkinProvidingContainer : SkinProvidingContainer
    {
        private Bindable<bool> beatmapSkins;
        private Bindable<bool> beatmapColours;
        private Bindable<bool> beatmapHitsounds;

        protected override bool AllowConfigurationLookup
        {
            get
            {
                if (beatmapSkins == null)
                    throw new InvalidOperationException($"{nameof(BeatmapSkinProvidingContainer)} needs to be loaded before being consumed.");

                return beatmapSkins.Value;
            }
        }

        protected override bool AllowColourLookup
        {
            get
            {
                if (beatmapColours == null)
                    throw new InvalidOperationException($"{nameof(BeatmapSkinProvidingContainer)} needs to be loaded before being consumed.");

                return beatmapColours.Value;
            }
        }

        protected override bool AllowDrawableLookup(ISkinComponent component)
        {
            if (beatmapSkins == null)
                throw new InvalidOperationException($"{nameof(BeatmapSkinProvidingContainer)} needs to be loaded before being consumed.");

            return beatmapSkins.Value;
        }

        protected override bool AllowTextureLookup(string componentName)
        {
            if (beatmapSkins == null)
                throw new InvalidOperationException($"{nameof(BeatmapSkinProvidingContainer)} needs to be loaded before being consumed.");

            return beatmapSkins.Value;
        }

        protected override bool AllowSampleLookup(ISampleInfo sampleInfo)
        {
            if (beatmapSkins == null)
                throw new InvalidOperationException($"{nameof(BeatmapSkinProvidingContainer)} needs to be loaded before being consumed.");

            return sampleInfo is StoryboardSampleInfo || beatmapHitsounds.Value;
        }

        public BeatmapSkinProvidingContainer(ISkin skin)
            : base(skin)
        {
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var config = parent.Get<OsuConfigManager>();

            beatmapSkins = config.GetBindable<bool>(OsuSetting.BeatmapSkins);
            beatmapColours = config.GetBindable<bool>(OsuSetting.BeatmapColours);
            beatmapHitsounds = config.GetBindable<bool>(OsuSetting.BeatmapHitsounds);

            return base.CreateChildDependencies(parent);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            beatmapSkins.BindValueChanged(_ => TriggerSourceChanged());
            beatmapColours.BindValueChanged(_ => TriggerSourceChanged());
            beatmapHitsounds.BindValueChanged(_ => TriggerSourceChanged());
        }
    }
}
