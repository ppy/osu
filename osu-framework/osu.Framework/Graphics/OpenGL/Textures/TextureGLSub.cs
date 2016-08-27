//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Runtime.InteropServices;
using System.Threading;
using OpenTK.Graphics.ES20;
using PixelFormat = OpenTK.Graphics.ES20.PixelFormat;
using System.Diagnostics;
using System.Drawing;
using OpenTK.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Batches;
using RectangleF = osu.Framework.Graphics.Primitives.RectangleF;

namespace osu.Framework.Graphics.OpenGL.Textures
{
    class TextureGLSub : TextureGL
    {
        private TextureGLSingle parent;
        private Rectangle bounds;

        private byte[] dataToBeUploaded = null;

        private int levelToBeUploaded;

        private PixelFormat formatToBeUploaded;

        public override int TextureId => parent.TextureId;
        public override bool Loaded => parent.Loaded || dataToBeUploaded != null;

        public TextureGLSub(Rectangle bounds, TextureGLSingle parent)
        {
            this.bounds = bounds;
            this.parent = parent;
        }

        public override int Height
        {
            get { return bounds.Height; }
            set { bounds.Height = value; }
        }

        public override int Width
        {
            get { return bounds.Width; }
            set { bounds.Width = value; }
        }

        /// <summary>
        /// Load texture data from a raw byte array (BGRA 32bit format)
        /// </summary>
        public override void SetData(byte[] data, int level = 0, PixelFormat format = PixelFormat.Rgba)
        {
            Debug.Assert(!isDisposed);

            lock (this)
            {
                formatToBeUploaded = format;
                levelToBeUploaded = level;
                dataToBeUploaded = data;
            }
        }

        /// <summary>
        /// Blits sprite to OpenGL display with specified parameters.
        /// </summary>
        public override void Draw(Quad vertexQuad, RectangleF? textureRect, Color4 drawColour, VertexBatch<TexturedVertex2d> spriteBatch = null)
        {
            Debug.Assert(!isDisposed);

            Upload();

            RectangleF actualBounds = bounds;

            if (textureRect.HasValue)
            {
                RectangleF localBounds = textureRect.Value;
                actualBounds.X += localBounds.X;
                actualBounds.Y += localBounds.Y;
                actualBounds.Width = Math.Min(localBounds.Width, bounds.Width);
                actualBounds.Height = Math.Min(localBounds.Height, bounds.Height);
            }

            parent.Draw(vertexQuad, actualBounds, drawColour, spriteBatch);
        }

        public override bool Upload()
        {
            if (isDisposed)
                return false;

            lock (this)
            {
                if (dataToBeUploaded != null)
                {
                    parent.SetData(dataToBeUploaded, bounds, levelToBeUploaded, formatToBeUploaded);
                    parent.Upload();
                    dataToBeUploaded = null;

                    return true;
                }
            }

            return false;
        }

        public override bool Bind()
        {
            Debug.Assert(!isDisposed);

            Upload();
            return parent.Bind();
        }
    }
}
