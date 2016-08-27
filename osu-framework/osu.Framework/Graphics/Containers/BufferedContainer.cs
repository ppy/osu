//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK;
using OpenTK.Graphics.ES20;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Buffers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using osu.Framework.Graphics.Batches;

namespace osu.Framework.Graphics.Containers
{
    class BufferedContainer : Container
    {
        private FrameBuffer frameBuffer;

        private QuadBatch<TexturedVertex2d> quadBatch = new QuadBatch<TexturedVertex2d>(1, 3);
        protected override IVertexBatch ActiveBatch => quadBatch;

        internal BufferedContainer()
        {
            frameBuffer = new FrameBuffer();
        }

        internal void Attach(RenderbufferInternalFormat format)
        {
            frameBuffer.Attach(format);
        }

        protected override void Update()
        {
            frameBuffer.Size = new Vector2(ScreenSpaceDrawQuad.Width, ScreenSpaceDrawQuad.Height);

            base.Update();
        }

        protected override void PreDraw()
        {
            frameBuffer.Bind();

            // Set viewport to the texture size
            GLWrapper.PushViewport(new Rectangle(0, 0, frameBuffer.Texture.Width, frameBuffer.Texture.Height));
            // We need to draw children as if they were zero-based to the top-left of the texture
            // so we make the new zero be this container's position without affecting children in any negative ways
            GLWrapper.PushOrtho(new Rectangle((int)ScreenSpaceDrawQuad.TopLeft.X, (int)ScreenSpaceDrawQuad.TopLeft.Y, frameBuffer.Texture.Width, frameBuffer.Texture.Height));

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        }

        protected override void PostDraw()
        {
            frameBuffer.Unbind();

            GLWrapper.PopOrtho();
            GLWrapper.PopViewport();

            GLWrapper.SetBlend(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);

            Rectangle textureRect = new Rectangle(0, frameBuffer.Texture.Height, frameBuffer.Texture.Width, -frameBuffer.Texture.Height);
            frameBuffer.Texture.Draw(ScreenSpaceDrawQuad, textureRect, DrawInfo.Colour, quadBatch);

            // In the case of nested framebuffer containerse we need to draw to
            // the last framebuffer container immediately, so let's force it
            ActiveBatch.Draw();
        }

        protected override void Dispose(bool isDisposing)
        {
            frameBuffer.Dispose();
            base.Dispose(isDisposing);
        }
    }
}
