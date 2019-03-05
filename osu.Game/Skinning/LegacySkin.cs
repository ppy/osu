// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Database;
using osu.Game.Graphics.Sprites;
using osuTK;

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

        protected LegacySkin(SkinInfo skin, IResourceStore<byte[]> storage, AudioManager audioManager, string filename)
            : base(skin)
        {
            Stream stream = storage.GetStream(filename);
            if (stream != null)
                using (StreamReader reader = new StreamReader(stream))
                    Configuration = new LegacySkinDecoder().Decode(reader);
            else
                Configuration = new SkinConfiguration();

            Samples = audioManager.GetSampleManager(storage);
            Textures = new TextureStore(new TextureLoaderStore(storage));
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
                case "Play/osu/number-text":
                    return !hasFont(Configuration.HitCircleFont)
                        ? null
                        : new LegacySpriteText(Textures, Configuration.HitCircleFont)
                        {
                            Scale = new Vector2(0.96f),
                            // Spacing value was reverse-engineered from the ratio of the rendered sprite size in the visual inspector vs the actual texture size
                            Spacing = new Vector2(-Configuration.HitCircleOverlap * 0.89f, 0)
                        };
            }

            var texture = GetTexture(componentName);

            if (texture == null)
                return null;

            return new Sprite { Texture = texture };
        }

        public override Texture GetTexture(string componentName)
        {
            float ratio = 2;

            var texture = Textures.Get($"{componentName}@2x");
            if (texture == null)
            {
                ratio = 1;
                texture = Textures.Get(componentName);
            }

            if (texture != null)
                texture.ScaleAdjust = ratio / 0.72f; // brings sizing roughly in-line with stable

            return texture;
        }

        public override SampleChannel GetSample(string sampleName) => Samples.Get(sampleName);

        private bool hasFont(string fontName) => GetTexture($"{fontName}-0") != null;

        protected class LegacySkinResourceStore<T> : IResourceStore<byte[]>
            where T : INamedFileInfo
        {
            private readonly IHasFiles<T> source;
            private readonly IResourceStore<byte[]> underlyingStore;

            private string getPathForFile(string filename)
            {
                bool hasExtension = filename.Contains('.');

                string lastPiece = filename.Split('/').Last();
                var legacyName = filename.StartsWith("Gameplay/taiko/") ? "taiko-" + lastPiece : lastPiece;

                var file = source.Files.Find(f =>
                    string.Equals(hasExtension ? f.Filename : Path.ChangeExtension(f.Filename, null), legacyName, StringComparison.InvariantCultureIgnoreCase));
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

            byte[] IResourceStore<byte[]>.Get(string name) => GetAsync(name).Result;

            public Task<byte[]> GetAsync(string name)
            {
                string path = getPathForFile(name);
                return path == null ? Task.FromResult<byte[]>(null) : underlyingStore.GetAsync(path);
            }

            #region IDisposable Support

            private bool isDisposed;

            protected virtual void Dispose(bool disposing)
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                }
            }

            ~LegacySkinResourceStore()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion
        }

        private class LegacySpriteText : OsuSpriteText
        {
            private readonly TextureStore textures;
            private readonly string font;

            public LegacySpriteText(TextureStore textures, string font)
            {
                this.textures = textures;
                this.font = font;

                Shadow = false;
                UseFullGlyphHeight = false;
            }

            protected override Texture GetTextureForCharacter(char c)
            {
                string textureName = $"{font}-{c}";

                // Approximate value that brings character sizing roughly in-line with stable
                float ratio = 36;

                var texture = textures.Get($"{textureName}@2x");
                if (texture == null)
                {
                    ratio = 18;
                    texture = textures.Get(textureName);
                }

                if (texture != null)
                    texture.ScaleAdjust = ratio;

                return texture;
            }
        }
    }
}
