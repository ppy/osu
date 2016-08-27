//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Runtime.InteropServices;
using System.Threading;
using OpenTK.Graphics.ES20;
using PixelFormat = OpenTK.Graphics.ES20.PixelFormat;
using System.Diagnostics;
using OpenTK.Graphics;
using OpenTK;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Primitives;
using System.Drawing;
using RectangleF = osu.Framework.Graphics.Primitives.RectangleF;

namespace osu.Framework.Graphics.OpenGL.Textures
{
    class TextureGLSingle : TextureGL
    {
        private static VertexBatch<TexturedVertex2d> spriteBatch;
        private Rectangle boundsToBeUploaded;

        private byte[] dataToBeUploaded = null;

        private int internalWidth;
        private int internalHeight;
        private int levelToBeUploaded;

        private TextureWrapMode internalWrapMode;
        private PixelFormat formatToBeUploaded;

        public override bool Loaded => textureId > 0 || dataToBeUploaded != null;

        public TextureGLSingle(int width, int height)
        {
            Width = width;
            Height = height;
        }

        #region Disposal
        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            unload();
        }

        /// <summary>
        /// Removes texture from GL memory.
        /// </summary>
        private void unload()
        {
            lock (this)
            {
                if (dataToBeUploaded != null)
                    FreeBuffer(dataToBeUploaded);
                dataToBeUploaded = null;
            }

            int disposableId = textureId;

            if (disposableId <= 0)
                return;

            GLWrapper.DeleteTextures(disposableId);

            textureId = 0;
        }
        #endregion

        private int height;
        public override int Height
        {
            get
            {
                Debug.Assert(!isDisposed);
                return height;
            }

            set
            {
                Debug.Assert(!isDisposed);
                height = value;
            }
        }

        private int width;
        public override int Width
        {
            get
            {
                Debug.Assert(!isDisposed);
                return width;
            }

            set
            {
                Debug.Assert(!isDisposed);
                width = value;
            }
        }

        private int textureId;
        public override int TextureId
        {
            get
            {
                Debug.Assert(!isDisposed);

                if (dataToBeUploaded != null)
                    Upload();

                return textureId;
            }
        }

        private static void RotateVector(ref Vector2 toRotate, float sin, float cos)
        {
            float oldX = toRotate.X;
            toRotate.X = toRotate.X * cos - toRotate.Y * sin;
            toRotate.Y = oldX * sin + toRotate.Y * cos;
        }

        /// <summary>
        /// Blits sprite to OpenGL display with specified parameters.
        /// </summary>
        public override void Draw(Quad vertexQuad, RectangleF? textureRect, Color4 drawColour, VertexBatch<TexturedVertex2d> spriteBatch = null)
        {
            Debug.Assert(!isDisposed);

            if (!Bind())
                return;

            RectangleF texRect = textureRect != null ?
                new RectangleF(textureRect.Value.X, textureRect.Value.Y, textureRect.Value.Width, textureRect.Value.Height) :
                new RectangleF(0, 0, Width, Height);

            texRect.X /= width;
            texRect.Y /= height;
            texRect.Width /= width;
            texRect.Height /= height;

            if (spriteBatch == null)
            {
                if (TextureGLSingle.spriteBatch == null)
                    TextureGLSingle.spriteBatch = new QuadBatch<TexturedVertex2d>(1, 100);
                spriteBatch = TextureGLSingle.spriteBatch;
            }

            spriteBatch.Add(new TexturedVertex2d() { Position = vertexQuad.BottomLeft, TexturePosition = new Vector2(texRect.Left, texRect.Bottom), Colour = drawColour });
            spriteBatch.Add(new TexturedVertex2d() { Position = vertexQuad.BottomRight, TexturePosition = new Vector2(texRect.Right, texRect.Bottom), Colour = drawColour });
            spriteBatch.Add(new TexturedVertex2d() { Position = vertexQuad.TopRight, TexturePosition = new Vector2(texRect.Right, texRect.Top), Colour = drawColour });
            spriteBatch.Add(new TexturedVertex2d() { Position = vertexQuad.TopLeft, TexturePosition = new Vector2(texRect.Left, texRect.Top), Colour = drawColour });
        }

        private void updateWrapMode()
        {
            Debug.Assert(!isDisposed);

            internalWrapMode = WrapMode;
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)internalWrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)internalWrapMode);
        }

        public void SetData(byte[] data, Rectangle bounds, int level = 0, PixelFormat format = PixelFormat.Rgba)
        {
            Debug.Assert(!isDisposed);

            lock (this)
            {
                if (dataToBeUploaded != null)
                    FreeBuffer(dataToBeUploaded);

                formatToBeUploaded = format;
                levelToBeUploaded = level;
                boundsToBeUploaded = bounds;
                dataToBeUploaded = data;

                IsTransparent = false;

                GLWrapper.EnqueueTextureUpload(this);
            }
        }

        /// <summary>
        /// Load texture data from a raw byte array (BGRA 32bit format)
        /// </summary>
        public override void SetData(byte[] data, int level = 0, PixelFormat format = PixelFormat.Rgba)
        {
            Debug.Assert(!isDisposed);
            SetData(data, new Rectangle(0, 0, width, height), level, format);
        }

        public override bool Bind()
        {
            Debug.Assert(!isDisposed);

            Upload();

            if (textureId <= 0)
                return false;

            if (IsTransparent)
                return false;

            GLWrapper.BindTexture(textureId);

            if (internalWrapMode != WrapMode)
                updateWrapMode();

            return true;
        }

        /// <summary>
        /// This is used for initializing power-of-two sized textures to transparent to avoid artifacts.
        /// </summary>
        private static byte[] transparentBlack = new byte[0];

        public override bool Upload()
        {
            // We should never run raw OGL calls on another thread than the main thread due to race conditions.
            //Debug.Assert(Game.MainThread == Thread.CurrentThread);

            //todo: thread safety via GLWrapper.

            if (isDisposed)
                return false;

            lock (this)
            {
                if (dataToBeUploaded == null)
                    return false;

                IntPtr dataPointer;
                GCHandle? h0;

                if (dataToBeUploaded.Length == 0)
                {
                    h0 = null;
                    dataPointer = IntPtr.Zero;
                }
                else
                {
                    h0 = GCHandle.Alloc(dataToBeUploaded, GCHandleType.Pinned);
                    dataPointer = h0.Value.AddrOfPinnedObject();
                }

                try
                {
                    // Do we need to generate a new texture?
                    if (textureId <= 0 || internalWidth < width || internalHeight < height)
                    {
                        internalWidth = width;
                        internalHeight = height;

                        // We only need to generate a new texture if we don't have one already. Otherwise just re-use the current one.
                        if (textureId <= 0)
                        {
                            int[] textures = new int[1];
                            GL.GenTextures(1, textures);

                            textureId = textures[0];

                            GLWrapper.BindTexture(textureId);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.LinearMipmapLinear);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.LinearMipmapLinear);

                            updateWrapMode();
                        }
                        else
                            GLWrapper.BindTexture(textureId);

                        if (width == boundsToBeUploaded.Width && height == boundsToBeUploaded.Height || dataPointer == IntPtr.Zero)
                            GL.TexImage2D(TextureTarget2d.Texture2D, levelToBeUploaded, TextureComponentCount.Rgba, width, height, 0, formatToBeUploaded, PixelType.UnsignedByte, dataPointer);
                        else
                        {
                            if (transparentBlack.Length < width * height * 4)
                                transparentBlack = new byte[width * height * 4]; // Default value is 0, exactly what we need.

                            GCHandle h1 = GCHandle.Alloc(transparentBlack, GCHandleType.Pinned);
                            GL.TexImage2D(TextureTarget2d.Texture2D, levelToBeUploaded, TextureComponentCount.Rgba, width, height, 0, formatToBeUploaded, PixelType.UnsignedByte, h1.AddrOfPinnedObject());
                            h1.Free();

                            GL.TexSubImage2D(TextureTarget2d.Texture2D, levelToBeUploaded, boundsToBeUploaded.X, boundsToBeUploaded.Y, boundsToBeUploaded.Width, boundsToBeUploaded.Height, formatToBeUploaded, PixelType.UnsignedByte, dataPointer);
                        }

                        GL.Hint(HintTarget.GenerateMipmapHint, HintMode.Nicest);
                        GL.GenerateMipmap(TextureTarget.Texture2D);
                    }
                    // Just update content of the current texture
                    else if (dataPointer != IntPtr.Zero)
                    {
                        GLWrapper.BindTexture(textureId);
                        int div = (int)Math.Pow(2, levelToBeUploaded);
                        GL.TexSubImage2D(TextureTarget2d.Texture2D, levelToBeUploaded, boundsToBeUploaded.X / div, boundsToBeUploaded.Y / div, boundsToBeUploaded.Width / div, boundsToBeUploaded.Height / div, formatToBeUploaded, PixelType.UnsignedByte, dataPointer);
                    }

                    return true;
                }
                finally
                {
                    if (h0.HasValue)
                        h0.Value.Free();

                    if (dataToBeUploaded != null)
                        FreeBuffer(dataToBeUploaded);

                    dataToBeUploaded = null;
                }
            }
        }
    }
}
