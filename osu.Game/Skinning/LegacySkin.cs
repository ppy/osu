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
using OpenTK;

namespace osu.Game.Skinning
{
    public class LegacySkin : Skin
    {
        protected TextureStore Textures;

        protected SampleManager Samples;

        public LegacySkin(SkinInfo skin, IResourceStore<byte[]> storage, AudioManager audioManager)
            : this(skin, new LegacySkinResourceStore<SkinFileInfo>(skin, storage), audioManager, "skin.ini")
        {
        }

        protected LegacySkin(SkinInfo skin, IResourceStore<byte[]> storage, AudioManager audioManager, string filename) : base(skin)
        {
            Stream stream = storage.GetStream(filename);
            if (stream != null)
                using (StreamReader reader = new StreamReader(stream))
                    Configuration = new LegacySkinDecoder().Decode(reader);
            else
                Configuration = new SkinConfiguration();

            Samples = audioManager.GetSampleManager(storage);
            Textures = new TextureStore(new RawTextureLoaderStore(storage));
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

            float ratio = 0.72f; // brings sizing roughly in-line with stable

            var texture = GetTexture($"{componentName}@2x");
            if (texture == null)
            {
                ratio *= 2;
                texture = GetTexture(componentName);
            }

            if (texture == null) return null;

            return new Sprite
            {
                Texture = texture,
                Scale = new Vector2(ratio),
            };
        }

        public override Texture GetTexture(string componentName) => Textures.Get(componentName);

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
                    string.Equals(hasExtension ? f.Filename : Path.ChangeExtension(f.Filename, null), lastPiece, StringComparison.InvariantCultureIgnoreCase));
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
