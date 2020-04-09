// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;

namespace osu.Game.Skinning
{
    public class LegacyBeatmapSkin : LegacySkin
    {
        protected override bool AllowManiaSkin => false;

        public LegacyBeatmapSkin(BeatmapInfo beatmap, IResourceStore<byte[]> storage, AudioManager audioManager)
            : base(createSkinInfo(beatmap), new LegacySkinResourceStore<BeatmapSetFileInfo>(beatmap.BeatmapSet, storage), audioManager, beatmap.Path)
        {
            // Disallow default colours fallback on beatmap skins to allow using parent skin combo colours. (via SkinProvidingContainer)
            Configuration.AllowDefaultComboColoursFallback = false;
        }

        public override IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
        {
            switch (lookup)
            {
                case LegacySkinConfiguration.LegacySetting s when s == LegacySkinConfiguration.LegacySetting.Version:
                    if (Configuration.LegacyVersion is decimal version)
                        return SkinUtils.As<TValue>(new Bindable<decimal>(version));

                    return null;
            }

            return base.GetConfig<TLookup, TValue>(lookup);
        }

        private static SkinInfo createSkinInfo(BeatmapInfo beatmap) =>
            new SkinInfo { Name = beatmap.ToString(), Creator = beatmap.Metadata.Author.ToString() };
    }
}
