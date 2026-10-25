// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Layout;

namespace osu.Game.Graphics.Sprites
{
    public partial class ColouredDimmableSprite : Sprite
    {
        public readonly ColouredDimmableHelper ColouredDimmableHelper;

        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            bool result = base.OnInvalidate(invalidation, source);

            // Needed because DrawColourOffset is handled by the DrawNode
            if ((invalidation & Invalidation.Colour) > 0)
            {
                result |= Invalidate(Invalidation.DrawNode);
            }

            return result;
        }

        public ColouredDimmableSprite()
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

        protected override void Update()
        {
            base.Update();

            TextureShader = ColouredDimmableHelper.GetShader(DrawColourInfo.Blending);
        }

        protected override DrawNode CreateDrawNode() => new ColouredDimmableSpriteDrawNode(this);

        public class ColouredDimmableSpriteDrawNode : SpriteDrawNode
        {
            public new ColouredDimmableSprite Source => (ColouredDimmableSprite)base.Source;

            public ColouredDimmableSpriteDrawNode(ColouredDimmableSprite source)
                : base(source)
            {
            }

            private ColouredDimmableSharedData colouredDimmableSharedData;

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
