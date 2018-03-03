// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;

namespace osu.Game.Skinning
{
    public class LegacySkin : Skin
    {
        private readonly TextureStore textures;

        private readonly SampleManager samples;

        public LegacySkin(SkinInfo skin, IResourceStore<byte[]> storage, AudioManager audioManager)
            : base(skin)
        {
            storage = new SkinResourceStore(skin, storage);
            samples = audioManager.GetSampleManager(storage);
            textures = new TextureStore(new RawTextureLoaderStore(storage));
        }

        public override Drawable GetDrawableComponent(string componentName)
        {
            var texture = textures.Get(componentName);
            if (texture == null) return null;

            return new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                Texture = texture,
            };
        }

        public override SampleChannel GetSample(string sampleName) => samples.Get(sampleName);
    }
}
