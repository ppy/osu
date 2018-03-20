// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using System.Linq;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Database;

namespace osu.Game.Skinning
{
    public class LegacySkin : Skin
    {
        protected TextureStore Textures;

        protected SampleManager Samples;

        public LegacySkin(SkinInfo skin, IResourceStore<byte[]> storage, AudioManager audioManager)
            : this(skin)
        {
            storage = new LegacySkinResourceStore<SkinFileInfo>(skin, storage);
            Samples = audioManager.GetSampleManager(storage);
            Textures = new TextureStore(new RawTextureLoaderStore(storage));

            Stream stream = storage.GetStream("skin.ini");
            if (stream != null)
                using (StreamReader reader = new StreamReader(stream))
                    Configuration = new LegacySkinDecoder().Decode(reader);
            else
                Configuration = new SkinConfiguration();
        }

        protected LegacySkin(SkinInfo skin) : base(skin)
        {
        }

        public override Drawable GetDrawableComponent(string componentName)
        {
            switch (componentName)
            {
                case "Play/Miss":
                    componentName = "hit0";
                    break;
                case "Play/Meh":
                    componentName = "hit50";
                    break;
                case "Play/Good":
                    componentName = "hit100";
                    break;
                case "Play/Great":
                    componentName = "hit300";
                    break;
            }

            var texture = Textures.Get(componentName);
            if (texture == null) return null;

            return new Sprite { Texture = texture };
        }

        public override SampleChannel GetSample(string sampleName) => Samples.Get(sampleName);

        protected class LegacySkinResourceStore<T> : IResourceStore<byte[]>
            where T : INamedFileInfo
        {
            private readonly IHasFiles<T> source;
            private readonly IResourceStore<byte[]> underlyingStore;

            private string getPathForFile(string filename)
            {
                bool hasExtension = filename.Contains('.');

                string lastPiece = filename.Split('/').Last();

                var file = source.Files.FirstOrDefault(f =>
                    string.Equals(hasExtension ? f.Filename : Path.GetFileNameWithoutExtension(f.Filename), lastPiece, StringComparison.InvariantCultureIgnoreCase));
                return file?.FileInfo.StoragePath;
            }

            public LegacySkinResourceStore(IHasFiles<T> source, IResourceStore<byte[]> underlyingStore)
            {
                this.source = source;
                this.underlyingStore = underlyingStore;
            }

            public Stream GetStream(string name)
            {
                string path = getPathForFile(name);
                return path == null ? null : underlyingStore.GetStream(path);
            }

            byte[] IResourceStore<byte[]>.Get(string name)
            {
                string path = getPathForFile(name);
                return path == null ? null : underlyingStore.Get(path);
            }
        }
    }
}
