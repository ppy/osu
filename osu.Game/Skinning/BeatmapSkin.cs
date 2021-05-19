// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Beatmaps;

namespace osu.Game.Skinning
{
    /// <summary>
    /// An empty implementation of a beatmap skin, serves as a temporary default for <see cref="WorkingBeatmap"/>s.
    /// </summary>
    /// <remarks>
    /// This should be removed once <see cref="Skin"/> becomes instantiable or a new skin type for osu!lazer beatmaps is defined.
    /// </remarks>
    public class BeatmapSkin : Skin
    {
        public BeatmapSkin(BeatmapInfo beatmap)
            : base(BeatmapSkinExtensions.CreateSkinInfo(beatmap), null)
        {
        }

        public override Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => null;

        public override IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => null;

        public override ISample GetSample(ISampleInfo sampleInfo) => null;
    }
}
