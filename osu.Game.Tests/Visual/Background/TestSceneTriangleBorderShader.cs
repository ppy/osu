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
    public class TestSceneTriangleBorderShader : OsuTestScene
    {
        public TestSceneTriangleBorderShader()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.DarkGreen
                },
                new TriangleBorder
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(100)
                }
            };
        }

        private class TriangleBorder : Sprite
        {
            [BackgroundDependencyLoader]
            private void load(ShaderManager shaders, IRenderer renderer)
            {
                TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "TriangleBorder");
                Texture = renderer.WhitePixel;
            }

            protected override DrawNode CreateDrawNode() => new TriangleBorderDrawNode(this);

            private class TriangleBorderDrawNode : SpriteDrawNode
            {
                public TriangleBorderDrawNode(TriangleBorder source)
                    : base(source)
                {
                }

                protected override bool CanDrawOpaqueInterior => false;
            }
        }
    }
}
