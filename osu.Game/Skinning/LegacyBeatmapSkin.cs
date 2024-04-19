// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.IO.Stores;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Objects.Types;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class LegacyBeatmapSkin : LegacySkin
    {
        protected override bool AllowManiaConfigLookups => false;
        protected override bool UseCustomSampleBanks => true;

        // matches stable. references:
        //  1. https://github.com/peppy/osu-stable-reference/blob/dc0994645801010d4b628fff5ff79cd3c286ca83/osu!/Graphics/Textures/TextureManager.cs#L115-L137 (beatmap skin textures lookup)
        //  2. https://github.com/peppy/osu-stable-reference/blob/dc0994645801010d4b628fff5ff79cd3c286ca83/osu!/Graphics/Textures/TextureManager.cs#L158-L196 (user skin textures lookup)
        protected override bool AllowHighResolutionSprites => false;

        /// <summary>
        /// Construct a new legacy beatmap skin instance.
        /// </summary>
        /// <param name="beatmapInfo">The model for this beatmap.</param>
        /// <param name="resources">Access to raw game resources.</param>
        public LegacyBeatmapSkin(BeatmapInfo beatmapInfo, IStorageResourceProvider? resources)
            : base(createSkinInfo(beatmapInfo), resources, createRealmBackedStore(beatmapInfo, resources), beatmapInfo.Path.AsNonNull())
        {
            // Disallow default colours fallback on beatmap skins to allow using parent skin combo colours. (via SkinProvidingContainer)
            Configuration.AllowDefaultComboColoursFallback = false;
        }

        private static IResourceStore<byte[]> createRealmBackedStore(BeatmapInfo beatmapInfo, IStorageResourceProvider? resources)
        {
            if (resources == null || beatmapInfo.BeatmapSet == null)
                // should only ever be used in tests.
                return new ResourceStore<byte[]>();

            return new RealmBackedResourceStore<BeatmapSetInfo>(beatmapInfo.BeatmapSet.ToLive(resources.RealmAccess), resources.Files, resources.RealmAccess);
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            if (lookup is SkinnableContainerLookup containerLookup)
            {
                switch (containerLookup.Target)
                {
                    case SkinnableContainerLookup.TargetArea.MainHUDComponents:
                        // this should exist in LegacySkin instead, but there isn't a fallback skin for LegacySkins yet.
                        // therefore keep the check here until fallback default legacy skin is supported.
                        if (!this.HasFont(LegacyFont.Score))
                            return null;

                        break;
                }
            }

            return base.GetDrawableComponent(lookup);
        }

        public override IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
        {
            switch (lookup)
            {
                case SkinConfiguration.LegacySetting s when s == SkinConfiguration.LegacySetting.Version:
                    // For lookup simplicity, ignore beatmap-level versioning completely.

                    // If it is decided that we need this due to beatmaps somehow using it, the default (1.0 specified in LegacySkinDecoder.CreateTemplateObject)
                    // needs to be removed else it will cause incorrect skin behaviours. This is due to the config lookup having no context of which skin
                    // it should be returning the version for.

                    LogLookupDebug(this, lookup, LookupDebugType.Miss);
                    return null;
            }

            return base.GetConfig<TLookup, TValue>(lookup);
        }

        protected override IBindable<Color4>? GetComboColour(IHasComboColours source, int comboIndex, IHasComboInformation combo)
            => base.GetComboColour(source, combo.ComboIndexWithOffsets, combo);

        public override ISample? GetSample(ISampleInfo sampleInfo)
        {
            if (sampleInfo is ConvertHitObjectParser.LegacyHitSampleInfo legacy && legacy.CustomSampleBank == 0)
            {
                // When no custom sample bank is provided, always fall-back to the default samples.
                return null;
            }

            return base.GetSample(sampleInfo);
        }

        private static SkinInfo createSkinInfo(BeatmapInfo beatmapInfo) =>
            new SkinInfo
            {
                Name = beatmapInfo.ToString(),
                Creator = beatmapInfo.Metadata.Author.Username
            };
    }
}
