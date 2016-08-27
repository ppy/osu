//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Resources;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace osu.Framework.Graphics.Textures
{
    public class TextureStore : ResourceStore<byte[]>
    {
        Dictionary<string, Texture> textureCache = new Dictionary<string, Texture>();

        private TextureAtlas atlas = new TextureAtlas(2048, 2048);

        public float ScaleAdjust = 1;

        public TextureStore(IResourceStore<byte[]> store) : base(store)
        {
        }

        /// <summary>
        /// Retrieves a texture from the store and adds it to the atlas.
        /// </summary>
        /// <param name="name">The name of the texture.</param>
        /// <returns>The texture.</returns>
        public new virtual Texture Get(string name)
        {
            Texture tex;

            //add file extension if it's missing.
            if (!name.Contains(@"."))
                name = name + @".png";

            if (textureCache.TryGetValue(name, out tex))
                tex = tex != null ? new Texture(tex.TextureGL) : null;
            else
            {
                textureCache[name] = tex = TextureLoader.FromBytes(base.Get(name), atlas);

                if (tex != null)
                {
                    //temporary test for availability of mipmap levels
                    int level = 1;
                    int div = 2;

                    while (tex.Width / div > 0)
                    {
                        byte[] by = base.Get($@"{name}/{div}");

                        if (by == null) break;

                        using (MemoryStream ms = new MemoryStream(by))
                        using (Bitmap b = (Bitmap)Image.FromStream(ms, false, false))
                        using (Bitmap b2 = new Bitmap(b, new Size(tex.Width / div, tex.Height / div)))
                            tex.SetData(b2, level);

                        level++;
                        div *= 2;
                    }
                }
            }

            if (tex != null && ScaleAdjust != 1)
            {
                tex.DpiScale = 1 / ScaleAdjust;
            }

            return tex;
        }
    }
}
