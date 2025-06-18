// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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

        protected override DrawNode CreateDrawNode() => new ColouredDimmableSpriteDrawNode(this);

        public class ColouredDimmableSpriteDrawNode : SpriteDrawNode
        {
            public new ColouredDimmableSprite Source => (ColouredDimmableSprite)base.Source;

            public ColouredDimmableSpriteDrawNode(ColouredDimmableSprite source)
                : base(source)
            {
            }

            private Colour4 drawColourOffset;

            public override void ApplyState()
            {
                base.ApplyState();

                drawColourOffset = (Source as IColouredDimmable).DrawColourOffset;
            }

            private IUniformBuffer<DimParameters> dimParametersBuffer;

            protected override void BindUniformResources(IShader shader, IRenderer renderer)
            {
                dimParametersBuffer ??= renderer.CreateUniformBuffer<DimParameters>();

                dimParametersBuffer.Data = new DimParameters
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
