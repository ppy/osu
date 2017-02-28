// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Desktop.Platform;
using osu.Framework.Desktop.Platform.Windows;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;

namespace osu.Game.Configuration
{
    public class SkinManager
    {
        public static Skin DEFAULT_SKIN = new Skin {            Name = @"default",
        };

        private Storage storage;

        private StorageResourceStore SkinsStore;
        public TextureStore Textures { get; private set; }
        // TODO audio component

        private Skin selected;

        public SkinManager()
        {
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host, OsuConfigManager config)
        {
            selected = config.Get<Skin>(OsuConfig.Skin);
            SkinsStore = new StorageResourceStore(host.Storage);
            UpdateSkin();
        }

        private void UpdateSkin()
        {
            Textures = new TextureStore();
            if (selected.Name != DEFAULT_SKIN.Name) {
                Textures.AddStore(new RawTextureLoaderStore(new NamespacedResourceStore<byte[]>(SkinsStore, Path.Combine("skins", selected.Path))));
            }
        }

        public class Skin
        {
            public string Name;
            public string Path;
        }
    }
}
