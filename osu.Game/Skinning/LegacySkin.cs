// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;

namespace osu.Game.Skinning
{
    public class LegacySkin : Skin
    {
        private readonly TextureStore textures;

        public LegacySkin(string name, Storage storage)
            : base(name)
        {
            textures = new TextureStore(new RawTextureLoaderStore(new StorageBackedResourceStore(storage.GetStorageForDirectory($"skins/{name}"))));
        }

        public override Drawable GetComponent(string componentName)
        {
            var legacyComponentName = componentName.Split('/').Last();

            var texture = textures.Get(legacyComponentName);
            if (texture == null) return null;

            return new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                Texture = textures.Get(legacyComponentName),
            };
        }
    }
}
