// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

// #nullable disable

using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Layout;

namespace osu.Game.Graphics.Sprites
{
    public partial class ColouredDimmableSprite : Sprite, IColouredDimmable
    {
        private readonly LayoutValue<IColouredDimmable> drawColourOffsetSource = new LayoutValue<IColouredDimmable>(Invalidation.Parent);

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
            TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "ColouredDimmableTexture");
        }

        public Colour4 DrawColourOffset
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

        public ColouredDimmableSharedData? ColouredDimmableSharedData
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

        protected override DrawNode CreateDrawNode() => new ColouredDimmableSpriteDrawNode(this);

        public class ColouredDimmableSpriteDrawNode : SpriteDrawNode
        {
            public new ColouredDimmableSprite Source => (ColouredDimmableSprite)base.Source;

            public ColouredDimmableSpriteDrawNode(ColouredDimmableSprite source)
                : base(source)
            {
            }

            private ColouredDimmableSharedData? colouredDimmableSharedData;

            private IUniformBuffer<DimParameters>? dimParametersBuffer;

            public override void ApplyState()
            {
                base.ApplyState();

                colouredDimmableSharedData = (Source as IColouredDimmable).ColouredDimmableSharedData;
            }

            protected override void BindUniformResources(IShader shader, IRenderer renderer)
            {
                if (colouredDimmableSharedData == null || colouredDimmableSharedData.OffsetParametersBuffer == null || DrawColourInfo.Blending == BlendingParameters.Additive)
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
