// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Video;

namespace osu.Game.Graphics.Sprites
{
    public partial class ColouredDimmableVideoSprite : VideoSprite
    {
        public readonly ColouredDimmableHelper ColouredDimmableHelper;

        private readonly Video video;

        public ColouredDimmableVideoSprite(Video video)
            : base(video)
        {
            this.video = video;

            ColouredDimmableHelper = new ColouredDimmableHelper(this);
            AddLayout(ColouredDimmableHelper.SourceLayout);
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            ColouredDimmableHelper.SetShaders(
                shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.VIDEO),
                shaders.Load(VertexShaderDescriptor.TEXTURE_2, "ColouredDimmableVideo")
            );
        }

        protected override void Update()
        {
            base.Update();

            TextureShader = ColouredDimmableHelper.GetShader(DrawColourInfo.Blending);
        }

        protected override DrawNode CreateDrawNode() => new ColouredDimmableVideoSpriteDrawNode(this, video);

        private class ColouredDimmableVideoSpriteDrawNode : VideoSpriteDrawNode
        {
            public new readonly ColouredDimmableVideoSprite Source;

            public ColouredDimmableVideoSpriteDrawNode(ColouredDimmableVideoSprite source, Video video)
                : base(video)
            {
                Source = source;
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
