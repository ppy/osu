// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;

namespace osu.Game.Graphics.Sprites
{
    public partial class ColouredDimmableBufferedContainer : BufferedContainer, IColouredDimmableSource
    {
        public readonly ColouredDimmableHelper ColouredDimmableHelper;

        public ColouredDimmableBufferedContainer(TexturePixelFormat textureFormat = TexturePixelFormat.R8G8B8A8Float, RenderBufferFormat[]? formats = null, bool pixelSnapping = false, bool cachedFrameBuffer = false)
            : base(textureFormat, formats, pixelSnapping, cachedFrameBuffer)
        {
            ColouredDimmableHelper = new ColouredDimmableHelper(this);
            AddLayout(ColouredDimmableHelper.SourceLayout);
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            ColouredDimmableHelper.SetShaders(
                shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE),
                shaders.Load(VertexShaderDescriptor.TEXTURE_2, "ColouredDimmableTexture")
            );
        }

        // Children should not receive the true colour to avoid colour doubling when the frame-buffers are rendered to the back-buffer.
        public ColouredDimmableSharedData? ColouredDimmableSharedData => null;

        protected override void Update()
        {
            base.Update();

            TextureShader = ColouredDimmableHelper.GetShader(DrawColourInfo.Blending);
        }

        protected override DrawNode CreateDrawNode() => new ColouredDimmableBufferedContainerDrawNode(this, SharedData);

        private class ColouredDimmableBufferedContainerDrawNode : BufferedContainerDrawNode
        {
            public new ColouredDimmableBufferedContainer Source => (ColouredDimmableBufferedContainer)base.Source;

            public ColouredDimmableBufferedContainerDrawNode(ColouredDimmableBufferedContainer source, BufferedContainerDrawNodeSharedData sharedData)
                : base(source, sharedData)
            {
            }

            private ColouredDimmableSharedData? colouredDimmableSharedData;

            public override void ApplyState()
            {
                base.ApplyState();

                colouredDimmableSharedData = Source.ColouredDimmableHelper.SharedData;
            }

            protected override void BindUniformResources(IShader shader, IRenderer renderer)
            {
                base.BindUniformResources(shader, renderer);

                if (colouredDimmableSharedData != null)
                {
                    shader.BindUniformBlock("m_DimParameters", colouredDimmableSharedData.GetBuffer(renderer));
                }
            }
        }
    }
}
