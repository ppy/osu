//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Resources;
using osu.Framework.Graphics.Textures;
using SharpFont;

namespace osu.Framework.Resources
{
    public class GlyphStore : IResourceStore<byte[]>
    {
        private static Library fontLibrary = new Library();

        private Face face;

        private string fontName;

        const float default_size = 96;

        public GlyphStore(byte[] font, string fontName = null)
        {
            this.fontName = fontName;
            face = new Face(fontLibrary, font, 0);
        }

        public byte[] Get(string name)
        {
            string[] parts = name.Split('/');
            return Get(parts[0], parts.Length == 1 ? 1 : 1f / Int32.Parse(parts[1]));
        }

        public byte[] Get(string name, float scale = 1)
        {
            face.SetCharSize(0, default_size * scale, 0, 96);

            if (!string.IsNullOrEmpty(fontName))
            {
                if (!name.StartsWith(fontName)) return null;
                name = name.Substring(fontName.Length + 1);
            }

            uint glyphIndex = face.GetCharIndex(name[0]);

            face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
            face.Glyph.RenderGlyph(RenderMode.Normal);

            if (face.Glyph.Bitmap.Buffer == IntPtr.Zero)
                return null;

            Bitmap glyphBitmap = face.Glyph.Bitmap.ToGdipBitmap(Color.White);
            BitmapData glyphBitmapData = glyphBitmap.LockBits(new Rectangle(0, 0, glyphBitmap.Width, glyphBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            int actualHeight = (int)(face.Glyph.Metrics.Height - face.Glyph.Metrics.HorizontalBearingY);
            int actualHeight2 = (int)(face.Glyph.Metrics.VerticalAdvance - face.Glyph.Metrics.Height);

            Bitmap glyphTexture = new Bitmap(glyphBitmap.Width, glyphBitmap.Height + actualHeight + actualHeight2, PixelFormat.Format32bppArgb);
            BitmapData glyphTextureData = glyphTexture.LockBits(new Rectangle(0, actualHeight + actualHeight2, glyphBitmap.Width, glyphBitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* srcData = (byte*)glyphBitmapData.Scan0;
                byte* destData = (byte*)glyphTextureData.Scan0;

                for (int y = 0; y < glyphBitmap.Height; y++)
                {
                    int srcOffset = y * glyphBitmapData.Stride;
                    int destOffset = y * glyphTextureData.Stride;

                    for (int x = 0; x < glyphBitmap.Width * 4; x += 4)
                    {
                        destData[destOffset + x] = 255;
                        destData[destOffset + x + 1] = 255;
                        destData[destOffset + x + 2] = 255;
                        destData[destOffset + x + 3] = srcData[srcOffset + (x / 4)];
                    }
                }
            }

            glyphBitmap.UnlockBits(glyphBitmapData);
            glyphTexture.UnlockBits(glyphTextureData);

            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(glyphTexture, typeof(byte[]));
        }

        public Stream GetStream(string name)
        {
            return new MemoryStream(Get(name));
        }
    }
}
