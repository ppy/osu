// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Rendering;
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Tests.Visual.Background
{
    public partial class TestSceneTriangleBorderShader : OsuTestScene
    {
        private readonly TestTriangle triangle;

        public TestSceneTriangleBorderShader()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.DarkGreen
                },
                triangle = new TestTriangle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(200)
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddSliderStep("Thickness", 0f, 1f, 0.15f, t => triangle.Thickness = t);
            AddSliderStep("Texel size", 0f, 0.1f, 0f, t => triangle.TexelSize = t);
        }

        private partial class TestTriangle : Sprite
        {
            private float thickness = 0.15f;

            public float Thickness
            {
                get => thickness;
                set
                {
                    thickness = value;
                    Invalidate(Invalidation.DrawNode);
                }
            }

            private float texelSize;

            public float TexelSize
            {
                get => texelSize;
                set
                {
                    texelSize = value;
                    Invalidate(Invalidation.DrawNode);
                }
            }

            [BackgroundDependencyLoader]
            private void load(ShaderManager shaders, IRenderer renderer)
            {
                TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "TriangleBorder");
                Texture = renderer.WhitePixel;
            }

            protected override DrawNode CreateDrawNode() => new TriangleDrawNode(this);

            private class TriangleDrawNode : SpriteDrawNode
            {
                public new TestTriangle Source => (TestTriangle)base.Source;

                public TriangleDrawNode(TestTriangle source)
                    : base(source)
                {
                }

                private float thickness;
                private float texelSize;

                public override void ApplyState()
                {
                    base.ApplyState();

                    thickness = Source.thickness;
                    texelSize = Source.texelSize;
                }

                private IUniformBuffer<TriangleBorderData>? borderDataBuffer;

                protected override void BindUniformResources(IShader shader, IRenderer renderer)
                {
                    base.BindUniformResources(shader, renderer);

                    borderDataBuffer ??= renderer.CreateUniformBuffer<TriangleBorderData>();
                    borderDataBuffer.Data = borderDataBuffer.Data with
                    {
                        Thickness = thickness,
                        TexelSize = texelSize
                    };

                    shader.BindUniformBlock("m_BorderData", borderDataBuffer);
                }

                protected override bool CanDrawOpaqueInterior => false;

                protected override void Dispose(bool isDisposing)
                {
                    base.Dispose(isDisposing);
                    borderDataBuffer?.Dispose();
                }
            }
        }
    }
}
