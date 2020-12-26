// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.IO.Stores;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.IO;
using osu.Game.Rulesets.Objects.Legacy;

namespace osu.Game.Skinning
{
    public class LegacyBeatmapSkin : LegacySkin
    {
        protected override bool AllowManiaSkin => false;
        protected override bool UseCustomSampleBanks => true;

        public LegacyBeatmapSkin(BeatmapInfo beatmap, IResourceStore<byte[]> storage, IStorageResourceProvider resources)
            : base(createSkinInfo(beatmap), new LegacySkinResourceStore<BeatmapSetFileInfo>(beatmap.BeatmapSet, storage), resources, beatmap.Path)
        {
            // Disallow default colours fallback on beatmap skins to allow using parent skin combo colours. (via SkinProvidingContainer)
            Configuration.AllowDefaultComboColoursFallback = false;
        }

        public override IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
        {
            switch (lookup)
            {
                case LegacySkinConfiguration.LegacySetting s when s == LegacySkinConfiguration.LegacySetting.Version:
                    // For lookup simplicity, ignore beatmap-level versioning completely.

                    // If it is decided that we need this due to beatmaps somehow using it, the default (1.0 specified in LegacySkinDecoder.CreateTemplateObject)
                    // needs to be removed else it will cause incorrect skin behaviours. This is due to the config lookup having no context of which skin
                    // it should be returning the version for.
                    return null;
            }

            return base.GetConfig<TLookup, TValue>(lookup);
        }

        public override SampleChannel GetSample(ISampleInfo sampleInfo)
        {
            if (sampleInfo is ConvertHitObjectParser.LegacyHitSampleInfo legacy && legacy.CustomSampleBank == 0)
            {
                // When no custom sample bank is provided, always fall-back to the default samples.
                return null;
            }

            return base.GetSample(sampleInfo);
        }

        private static SkinInfo createSkinInfo(BeatmapInfo beatmap) =>
            new SkinInfo { Name = beatmap.ToString(), Creator = beatmap.Metadata.Author.ToString() };
    }
}
