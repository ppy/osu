//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using osu.Framework.Graphics.OpenGL.Textures;

namespace osu.Framework.Graphics.Textures
{
    public class TextureAtlas
    {
        private List<Rectangle> subTextureBounds = new List<Rectangle>();
        private TextureGLSingle atlasTexture;

        private int atlasWidth;
        private int atlasHeight;

        private int currentY = 0;

        private bool IsFull
        {
            get
            {
                return false;
            }
        }

        public TextureAtlas(int width, int height)
        {
            this.atlasWidth = width;
            this.atlasHeight = height;
        }

        public void Reset()
        {
            subTextureBounds.Clear();
            currentY = 0;

            atlasTexture = new TextureGLSingle(atlasWidth, atlasHeight);
        }

        private Point FindPosition(int width, int height)
        {
            // Super naive implementation only going from left to right.
            Point res = new Point(0, currentY);

            if (currentY + height > atlasHeight)
            {
                Reset();
                return new Point(0, 0);
            }

            int maxY = currentY;
            foreach (Rectangle bounds in subTextureBounds)
            {
                // +1 is required to prevent aliasing issues with sub-pixel positions while drawing. Bordering edged of other textures can show without it.
                res.X = Math.Max(res.X, bounds.Right + 1);
                maxY = Math.Max(maxY, bounds.Bottom);
            }

            if (res.X + width > atlasWidth)
            {
                // +1 is required to prevent aliasing issues with sub-pixel positions while drawing. Bordering edged of other textures can show without it.
                currentY = maxY + 1;
                subTextureBounds.Clear();
                res = FindPosition(width, height);
            }

            return res;
        }

        internal Texture Add(int width, int height)
        {
            lock(this)
            {
                if (atlasTexture == null)
                    Reset();

                Point position = FindPosition(width, height);
                Rectangle bounds = new Rectangle(position.X, position.Y, width, height);
                subTextureBounds.Add(bounds);
                
                return new Texture(new TextureGLSub(bounds, atlasTexture));
            }
        }

    }
}
