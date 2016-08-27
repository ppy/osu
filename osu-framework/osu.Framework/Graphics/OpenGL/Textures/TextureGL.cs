//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Runtime.InteropServices;
using System.Threading;
using OpenTK.Graphics.ES20;
using PixelFormat = OpenTK.Graphics.ES20.PixelFormat;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using OpenTK.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Batches;
using RectangleF = osu.Framework.Graphics.Primitives.RectangleF;

namespace osu.Framework.Graphics.OpenGL.Textures
{
    public abstract class TextureGL : IDisposable
    {
        public bool IsTransparent = false;
        public TextureWrapMode WrapMode = TextureWrapMode.ClampToEdge;

        #region Disposal
        ~TextureGL()
        {
            Dispose(false);
        }

        protected bool isDisposed = false;
        protected virtual void Dispose(bool isDisposing)
        {
            isDisposed = true;
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            GC.SuppressFinalize(this);
            Dispose(true);
        }
        #endregion

        public abstract bool Loaded
        {
            get;
        }

        public abstract int TextureId
        {
            get;
        }

        public abstract int Height
        {
            get;
            set;
        }

        public abstract int Width
        {
            get;
            set;
        }

        /// <summary>
        /// Blits sprite to OpenGL display with specified parameters.
        /// </summary>
        public abstract void Draw(Quad vertexQuad, RectangleF? textureRect, Color4 drawColour, VertexBatch<TexturedVertex2d> spriteBatch = null);

        /// <summary>
        /// Bind as active texture.
        /// </summary>
        /// <returns>True if bind was successful.</returns>
        public abstract bool Bind();

        /// <summary>
        /// Uploads pending texture data to the GPU if it exists.
        /// </summary>
        /// <returns>Whether pending data existed and an upload has been performed.</returns>
        public abstract bool Upload();

        /// <summary>
        /// Load texture data from a raw byte array (BGRA 32bit format)
        /// </summary>
        public abstract void SetData(byte[] data, int level = 0, PixelFormat format = PixelFormat.Rgba);

        /// <summary>
        /// Load texture data from a raw IntPtr location (BGRA 32bit format)
        /// </summary>
        public void SetData(IntPtr dataPointer, int level = 0, PixelFormat format = 0)
        {
            Debug.Assert(!isDisposed);

            if (format == 0)
                format = PixelFormat.Rgba;

            byte[] data;
            if (dataPointer == IntPtr.Zero)
            {
                data = new byte[0];
            }
            else
            {
                data = ReserveBuffer(Width * Height * 4);
                Marshal.Copy(dataPointer, data, 0, data.Length);
            }

            SetData(data, level, format);
        }

        private const int MAX_AMOUNT_DATA_BUFFERS = 10;
        private static Stack<byte[]> freeDataBuffers = new Stack<byte[]>();
        private static HashSet<byte[]> usedDataBuffers = new HashSet<byte[]>();

        private static byte[] findFreeBuffer(int minimumLength)
        {
            byte[] buffer = null;

            if (freeDataBuffers.Count > 0)
                buffer = freeDataBuffers.Pop();

            if (buffer == null || buffer.Length < minimumLength)
                buffer = new byte[minimumLength];

            if (usedDataBuffers.Count < MAX_AMOUNT_DATA_BUFFERS)
                usedDataBuffers.Add(buffer);

            return buffer;
        }

        private static void returnFreeBuffer(byte[] buffer)
        {
            if (usedDataBuffers.Remove(buffer))
                // We are here if the element was successfully found and removed
                freeDataBuffers.Push(buffer);
        }

        /// <summary>
        /// Reserve a buffer from the texture buffer pool. This is used to avoid excessive amounts of heap allocations.
        /// </summary>
        /// <param name="minimumLength">The minimum length required of the reserved buffer.</param>
        /// <returns>The reserved buffer.</returns>
        public static byte[] ReserveBuffer(int minimumLength)
        {
            byte[] buffer;
            lock (freeDataBuffers)
                buffer = findFreeBuffer(minimumLength);

            return buffer;
        }

        /// <summary>
        /// Frees a previously reserved buffer for future reservations.
        /// </summary>
        /// <param name="buffer">The buffer to be freed. If the buffer has not previously been reserved then this method does nothing.</param>
        public static void FreeBuffer(byte[] buffer)
        {
            lock (freeDataBuffers)
                returnFreeBuffer(buffer);
        }
    }
}
