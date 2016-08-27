//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK;
using OpenTK.Graphics.ES20;
using osu.Framework.Graphics.OpenGL.Textures;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Framework.Graphics.OpenGL.Buffers
{
    class FrameBuffer : IDisposable
    {
        private int lastFramebuffer;
        private int frameBuffer = -1;

        internal TextureGL Texture { get; private set; }

        private bool IsBound => lastFramebuffer != -1;

        private List<RenderBuffer> attachedRenderBuffers = new List<RenderBuffer>();

        internal FrameBuffer(bool withTexture = true)
        {
            frameBuffer = GL.GenFramebuffer();

            if (withTexture)
            {
                Texture = new TextureGLSingle(1, 1);
                Texture.SetData(new byte[0]);
                Texture.Upload();

                Bind();

                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, TextureTarget.Texture2D, Texture.TextureId, 0);
                GLWrapper.BindTexture(0);

                Unbind();
            }
        }

        #region Disposal
        ~FrameBuffer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        private bool isDisposed;
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
                return;
            isDisposed = true;

            Unbind();

            GLWrapper.DeleteFramebuffer(frameBuffer);
            frameBuffer = -1;
        }
        #endregion

        private Vector2 size = Vector2.One;
        /// <summary>
        /// Sets the size of the texture of this framebuffer.
        /// </summary>
        internal Vector2 Size
        {
            get { return size; }
            set
            {
                if (value == size)
                    return;
                size = value;

                Texture.Width = (int)Math.Ceiling(size.X);
                Texture.Height = (int)Math.Ceiling(size.Y);
                Texture.SetData(new byte[0]);
                Texture.Upload();
            }
        }

        /// <summary>
        /// Attaches a RenderBuffer to this framebuffer.
        /// </summary>
        /// <param name="format">The type of RenderBuffer to attach.</param>
        internal void Attach(RenderbufferInternalFormat format)
        {
            if (attachedRenderBuffers.Exists(r => r.Format == format))
                return;

            attachedRenderBuffers.Add(new RenderBuffer(format));
        }

        /// <summary>
        /// Binds the framebuffer.
        /// <para>Does not clear the buffer or reset the viewport/ortho.</para>
        /// </summary>
        internal void Bind()
        {
            if (frameBuffer == -1)
                return;

            if (lastFramebuffer == frameBuffer)
                return;

            // Bind framebuffer and all its renderbuffers
            lastFramebuffer = GLWrapper.BindFrameBuffer(frameBuffer);
            attachedRenderBuffers.ForEach(r => r.Bind(frameBuffer));
        }

        /// <summary>
        /// Unbinds the framebuffer.
        /// </summary>
        internal void Unbind()
        {
            if (!IsBound)
                return;

            GLWrapper.BindFrameBuffer(lastFramebuffer);
            attachedRenderBuffers.ForEach(r => r.Unbind());

            lastFramebuffer = -1;
        }
    }
}
