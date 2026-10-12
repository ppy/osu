// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

// #nullable disable

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
    public partial class ColouredDimmableBufferedContainer : BufferedContainer, IColouredDimmable
    {
        private readonly LayoutValue<IColouredDimmable> drawColourOffsetSource = new LayoutValue<IColouredDimmable>(Invalidation.Parent);

        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            bool result = false;

            // Needed because DrawColourOffset is handled by the DrawNode
            if ((invalidation & Invalidation.Colour) > 0)
            {
                result |= Invalidate(Invalidation.DrawNode);
            }

            // Colour is handled by ColouredDimmableBufferedContainer, no need to propagate it to children
            invalidation &= ~Invalidation.Colour;

            result |= base.OnInvalidate(invalidation, source);

            return result;
        }

        public ColouredDimmableBufferedContainer(TexturePixelFormat textureFormat = TexturePixelFormat.R8G8B8A8Float, RenderBufferFormat[] formats = null, bool pixelSnapping = false, bool cachedFrameBuffer = false)
            : base(textureFormat, formats, pixelSnapping, cachedFrameBuffer)
        {
            AddLayout(drawColourOffsetSource);
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "ColouredDimmableTexture");
        }

        // Children should not receive the true colour to avoid colour doubling when the frame-buffers are rendered to the back-buffer.
        public Colour4 DrawColourOffset => Colour4.Black;

        public ColouredDimmableSharedData? ColouredDimmableSharedData => null;

        public Colour4 FrameBufferDrawColourOffset
        {
            get
            {
                if (DrawColourInfo.Blending == BlendingParameters.Additive)
                    // Additive Drawables shouldn't have any offset, because things underneath them
                    // already have it added to them. This does rely on the fact that there is
                    // something non-additive (but still IColouredDimmable) behind this sprite,
                    // which seems to be the case with storyboards, as beatmap background is always enabled.
                    return Colour4.Black;

                if (!drawColourOffsetSource.IsValid)
                    drawColourOffsetSource.Value = (this as IColouredDimmable).DrawColourOffsetSource;

                if (drawColourOffsetSource.Value != null && drawColourOffsetSource.Value != this)
                    return drawColourOffsetSource.Value.DrawColourOffset;

                return Colour4.Black;
            }
        }

        public ColouredDimmableSharedData? FrameBufferColouredDimmableSharedData
        {
            get
            {
                if (!drawColourOffsetSource.IsValid)
                    drawColourOffsetSource.Value = (this as IColouredDimmable).DrawColourOffsetSource;

                if (drawColourOffsetSource.Value != null && drawColourOffsetSource.Value != this)
                    return drawColourOffsetSource.Value.ColouredDimmableSharedData;

                return null;
            }
        }

        protected override DrawNode CreateDrawNode() => new ColouredDimmableBufferedContainerDrawNode(this, SharedData);

        protected class ColouredDimmableBufferedContainerDrawNode : BufferedContainerDrawNode
        {
            public new ColouredDimmableBufferedContainer Source => (ColouredDimmableBufferedContainer)base.Source;

            public ColouredDimmableBufferedContainerDrawNode(ColouredDimmableBufferedContainer source, BufferedContainerDrawNodeSharedData sharedData)
                : base(source, sharedData)
            {
            }

            private ColouredDimmableSharedData? colouredDimmableSharedData;

            private IUniformBuffer<DimParameters>? dimParametersBuffer;

            public override void ApplyState()
            {
                base.ApplyState();

                colouredDimmableSharedData = Source.FrameBufferColouredDimmableSharedData;
            }

            protected override void BindUniformResources(IShader shader, IRenderer renderer)
            {
                if (colouredDimmableSharedData == null || colouredDimmableSharedData.OffsetParametersBuffer == null || colouredDimmableSharedData.ZeroOffsetParametersBuffer == null)
                {
                    dimParametersBuffer ??= renderer.CreateUniformBuffer<DimParameters>();

                    dimParametersBuffer.Data = new DimParameters
                    {
                        DimColour = new UniformVector4
                        {
                            X = 0.0f,
                            Y = 0.0f,
                            Z = 0.0f,
                            W = 1.0f
                        },
                    };

                    shader.BindUniformBlock("m_DimParameters", dimParametersBuffer);
                }
                else if (DrawColourInfo.Blending == BlendingParameters.Additive)
                {
                    // Additive Drawables shouldn't have any offset, because things underneath them
                    // already have it added to them. This does rely on the fact that there is
                    // something non-additive (but still IColouredDimmable) behind this sprite,
                    // which seems to be the case with storyboards, as beatmap background is always enabled.
                    shader.BindUniformBlock("m_DimParameters", colouredDimmableSharedData.ZeroOffsetParametersBuffer);
                }
                else
                {
                    shader.BindUniformBlock("m_DimParameters", colouredDimmableSharedData.OffsetParametersBuffer);
                }
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                dimParametersBuffer?.Dispose();
            }
        }
    }
}
