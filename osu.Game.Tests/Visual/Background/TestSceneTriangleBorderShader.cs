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

namespace osu.Game.Tests.Visual.Background
{
    public partial class TestSceneTriangleBorderShader : OsuTestScene
    {
        private readonly TriangleBorder border;

        public TestSceneTriangleBorderShader()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.DarkGreen
                },
                border = new TriangleBorder
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(100)
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddSliderStep("Thickness", 0f, 1f, 0.02f, t => border.Thickness = t);
        }

        private partial class TriangleBorder : Sprite
        {
            private float thickness = 0.02f;

            public float Thickness
            {
                get => thickness;
                set
                {
                    thickness = value;
                    Invalidate(Invalidation.DrawNode);
                }
            }

            [BackgroundDependencyLoader]
            private void load(ShaderManager shaders, IRenderer renderer)
            {
                TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "TriangleBorder");
                Texture = renderer.WhitePixel;
            }

            protected override DrawNode CreateDrawNode() => new TriangleBorderDrawNode(this);

            private class TriangleBorderDrawNode : SpriteDrawNode
            {
                public new TriangleBorder Source => (TriangleBorder)base.Source;

                public TriangleBorderDrawNode(TriangleBorder source)
                    : base(source)
                {
                }

                private float thickness;

                public override void ApplyState()
                {
                    base.ApplyState();

                    thickness = Source.thickness;
                }

                public override void Draw(IRenderer renderer)
                {
                    TextureShader.GetUniform<float>("thickness").UpdateValue(ref thickness);

                    base.Draw(renderer);
                }

                protected override bool CanDrawOpaqueInterior => false;
            }
        }
    }
}
