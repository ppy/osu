// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;

namespace osu.Game.Skins
{
    public class Skin
    {
        public SkinInfo info;
        public TextureStore Textures { get; private set; }

        private StorageResourceStore SkinsStore;
        private Storage storage;

        public Skin(SkinInfo info)
        {
            this.info = info;
        }

        public void UpdateSkin() {
            Textures = new TextureStore();
            if (info.Name != SkinManager.DEFAULT_SKIN.Name)
            {
                Textures.AddStore(new RawTextureLoaderStore(new NamespacedResourceStore<byte[]>(SkinsStore, Path.Combine("skins", info.Path))));
            }
            // TODO update audio component
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host) {
            storage = host.Storage;
            SkinsStore = new StorageResourceStore(storage);
            UpdateSkin();
        }
    }

    public class SkinInfo
    {
        public string Name;
        public string Path;
    }
}
