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
        private readonly LayoutValue<IColouredDimmableSource> drawColourOffsetSource = new LayoutValue<IColouredDimmableSource>(Invalidation.Parent);

        private IShader originalShader;

        private IShader colouredShader;

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
            : base()
        {
            AddLayout(drawColourOffsetSource);
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            originalShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE);
            colouredShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "ColouredDimmableTexture");
        }

        public ColouredDimmableSharedData ColouredDimmableSharedData
        {
            get
            {
                if (!drawColourOffsetSource.IsValid)
                    drawColourOffsetSource.Value = this.FindClosestParent<IColouredDimmableSource>();

                if (drawColourOffsetSource.Value != null)
                    return drawColourOffsetSource.Value.ColouredDimmableSharedData;

                return null;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (DrawColourInfo.Blending == BlendingParameters.Additive || ColouredDimmableSharedData == null)
            {
                // Additive Drawables shouldn't have any offset, because things underneath them
                // already have it added to them. This does rely on the fact that there is
                // something non-additive (but still IColouredDimmable) behind this sprite,
                // which seems to be the case with storyboards, as beatmap background is always enabled.
                TextureShader = originalShader;
            }
            else
            {
                TextureShader = colouredShader;
            }
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

                colouredDimmableSharedData = Source.ColouredDimmableSharedData;
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
