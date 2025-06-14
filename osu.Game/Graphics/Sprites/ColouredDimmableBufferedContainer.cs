// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Layout;

namespace osu.Game.Graphics.Sprites
{
    public partial class DimmableBufferedContainer : BufferedContainer, IColouredDimmable
    {
        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            // Colour is handled by BufferedContainer, no need to propagate it to children
            // Perhaps this needs to be moved into base class
            invalidation &= ~Invalidation.Colour;

            bool result = base.OnInvalidate(invalidation, source);

            if ((invalidation & Invalidation.Colour) > 0)
            {
                result |= Invalidate(Invalidation.DrawNode);
            }

            return result;
        }

        public DimmableBufferedContainer(RenderBufferFormat[] formats = null, bool pixelSnapping = false, bool cachedFrameBuffer = false)
            : base(formats, pixelSnapping, cachedFrameBuffer)
        {
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "ColouredDimmableTexture");
        }

        public Colour4 FrameBufferDrawColourOffset
        {
            get
            {
                if (Parent is IColouredDimmable colouredDimmableParent)
                    return colouredDimmableParent.DrawColourOffset;
                else if (Parent?.Parent is IColouredDimmable colouredDimmableGrandparent)
                    // Needed to skip intermediate containers.
                    return colouredDimmableGrandparent.DrawColourOffset;

                return Colour4.Black;
            }
        }

        // Children should not receive the true colour to avoid colour doubling when the frame-buffers are rendered to the back-buffer.
        public Colour4 DrawColourOffset => Colour4.Black;

        protected override DrawNode CreateDrawNode() => new DimmableBufferedContainerDrawNode(this, SharedData);

        protected class DimmableBufferedContainerDrawNode : BufferedContainerDrawNode
        {
            public new DimmableBufferedContainer Source => (DimmableBufferedContainer)base.Source;

            public DimmableBufferedContainerDrawNode(DimmableBufferedContainer source, BufferedContainerDrawNodeSharedData sharedData)
                : base(source, sharedData)
            {
            }

            private Colour4 drawColourOffset;

            public override void ApplyState()
            {
                base.ApplyState();

                drawColourOffset = Source.FrameBufferDrawColourOffset;
            }

            private IUniformBuffer<DimParameters> dimParametersBuffer;

            protected override void BindUniformResources(IShader shader, IRenderer renderer)
            {
                dimParametersBuffer ??= renderer.CreateUniformBuffer<DimParameters>();

                dimParametersBuffer.Data = dimParametersBuffer.Data with
                {
                    DimColour = new UniformVector4
                    {
                        X = drawColourOffset.R,
                        Y = drawColourOffset.G,
                        Z = drawColourOffset.B,
                        W = drawColourOffset.A
                    },
                };

                shader.BindUniformBlock("m_DimParameters", dimParametersBuffer);
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                dimParametersBuffer?.Dispose();
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            private record struct DimParameters
            {
                public UniformVector4 DimColour;
            }
        }
    }
}
