// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;
using osu.Game.Beatmaps.IO;
using SQLite.Net.Attributes;

namespace osu.Game.Skins
{
    public class Skin
    {
        public SkinInfo Info { get; private set; }
        public TextureStore Textures { get; private set; }

        private ArchiveReader skinsStore;

        public Skin(SkinInfo info)
        {
            Info = info;
        }

        public void UpdateSkin(Storage storage) {
            if (Info.Path != null)
            {
                skinsStore = ArchiveReader.GetReader(storage, Info.Path);

                Textures = new TextureStore();
                if (Info.Name != SkinManager.DefaultSkin.Name)
                {
                    Textures.AddStore(new RawTextureLoaderStore(skinsStore));
                }
                // TODO update audio component
            }
        }
    }

    public class SkinInfo
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public string Name { get; set; }
        public string Path { get; set; }
    }
}
