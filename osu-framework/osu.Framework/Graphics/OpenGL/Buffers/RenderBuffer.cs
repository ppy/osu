//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK.Graphics.ES20;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace osu.Framework.Graphics.OpenGL.Buffers
{
    class RenderBuffer : IDisposable
    {
        private static Dictionary<RenderbufferInternalFormat, ConcurrentStack<RenderBufferInfo>> renderBufferCache = new Dictionary<RenderbufferInternalFormat, ConcurrentStack<RenderBufferInfo>>();

        private RenderBufferInfo info;
        private bool isDisposed;

        internal RenderbufferInternalFormat Format { get; private set; }

        internal RenderBuffer(RenderbufferInternalFormat format)
        {
            this.Format = format;

            info.ID = -1;
            info.LastFramebuffer = -1;
        }

        #region Disposal
        ~RenderBuffer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
                return;
            isDisposed = true;

            Unbind();
        }
        #endregion

        /// <summary>
        /// Binds the renderbuffer to the specfied framebuffer.
        /// </summary>
        /// <param name="frameBuffer">The framebuffer this renderbuffer should be bound to.</param>
        internal void Bind(int frameBuffer)
        {
            if (info.ID != -1)
                return;

            if (!renderBufferCache.ContainsKey(Format))
                renderBufferCache[Format] = new ConcurrentStack<RenderBufferInfo>();

            // Make sure we have renderbuffers available
            if (renderBufferCache[Format].Count == 0)
            {
                int newBuffer = GL.GenRenderbuffer();
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, newBuffer);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, Format, Game.Window.Width, Game.Window.Height);

                renderBufferCache[Format].Push(new RenderBufferInfo() { ID = newBuffer, LastFramebuffer = -1 });
            }

            // Get a renderbuffer from the cache
            renderBufferCache[Format].TryPop(out info);

            // For performance reasons, we only need to re-bind the renderbuffer to
            // the framebuffer if it is not already attached to it
            if (info.LastFramebuffer != frameBuffer)
            {
                // Make sure the framebuffer we want to attach to is bound
                int lastFrameBuffer = GLWrapper.BindFrameBuffer(frameBuffer);

                switch (Format)
                {
                    case RenderbufferInternalFormat.DepthComponent16:
                        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.DepthAttachment, RenderbufferTarget.Renderbuffer, info.ID);
                        break;
                    case RenderbufferInternalFormat.Rgb565:
                    case RenderbufferInternalFormat.Rgb5A1:
                    case RenderbufferInternalFormat.Rgba4:
                        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, RenderbufferTarget.Renderbuffer, info.ID);
                        break;
                    case RenderbufferInternalFormat.StencilIndex8:
                        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.DepthAttachment, RenderbufferTarget.Renderbuffer, info.ID);
                        break;
                }

                GLWrapper.BindFrameBuffer(lastFrameBuffer);
            }

            info.LastFramebuffer = frameBuffer;
        }

        /// <summary>
        /// Unbinds the renderbuffer.
        /// <para>The renderbuffer will remain internally attached to the framebuffer.</para>
        /// </summary>
        internal void Unbind()
        {
            // Return the renderbuffer to the cache
            renderBufferCache[Format].Push(info);
            info.ID = -1;
        }

        private struct RenderBufferInfo
        {
            public int ID;
            public int LastFramebuffer;
        }
    }
}
