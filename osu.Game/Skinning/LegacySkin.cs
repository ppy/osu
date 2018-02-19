// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
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

        public SkinInfo SkinInfo;

        public LegacySkin(SkinInfo skin, IResourceStore<byte[]> storage)
            : base(skin.Name)
        {
            SkinInfo = skin;
            textures = new TextureStore(new RawTextureLoaderStore(storage));
        }

        private string getPathForFile(string filename) => SkinInfo.Files.First(f => string.Equals(f.Filename, filename, StringComparison.InvariantCultureIgnoreCase)).FileInfo.StoragePath;

        public override Drawable GetDrawableComponent(string componentName)
        {
            var legacyComponentName = componentName.Split('/').Last();

            var texture = textures.Get(getPathForFile(legacyComponentName));
            if (texture == null) return null;

            return new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                Texture = texture,
            };
        }

        public override SampleChannel GetSample(string sampleName) => null;
    }
}
