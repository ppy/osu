// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using System.Linq;
using osu.Framework.IO.Stores;

namespace osu.Game.Skinning
{
    public class SkinResourceStore : IResourceStore<byte[]>
    {
        private readonly SkinInfo skin;
        private readonly IResourceStore<byte[]> underlyingStore;

        private string getPathForFile(string filename) =>
            skin.Files.FirstOrDefault(f => string.Equals(Path.GetFileNameWithoutExtension(f.Filename), filename.Split('/').Last(), StringComparison.InvariantCultureIgnoreCase))?.FileInfo.StoragePath;

        public SkinResourceStore(SkinInfo skin, IResourceStore<byte[]> underlyingStore)
        {
            this.skin = skin;
            this.underlyingStore = underlyingStore;
        }

        public Stream GetStream(string name) => underlyingStore.GetStream(getPathForFile(name));

        byte[] IResourceStore<byte[]>.Get(string name) => underlyingStore.Get(getPathForFile(name));
    }
}
