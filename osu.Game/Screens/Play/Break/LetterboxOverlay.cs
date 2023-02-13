// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osuTK;

namespace osu.Game.Screens.Play.Break
{
    public partial class LetterboxOverlay : Drawable
    {
        private const int height = 350;

        public LetterboxOverlay()
        {
            RelativeSizeAxes = Axes.Both;
        }

        private IShader shader = null!;

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            shader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "LetterboxOverlay");
        }

        protected override DrawNode CreateDrawNode() => new LetterboxOverlayDrawNode(this);

        protected class LetterboxOverlayDrawNode : DrawNode
        {
            public new LetterboxOverlay Source => (LetterboxOverlay)base.Source;

            public LetterboxOverlayDrawNode(LetterboxOverlay source)
                : base(source)
            {
            }

            private IShader shader = null!;
            private float relativeHeight;
            private Vector2 drawSize;

            public override void ApplyState()
            {
                base.ApplyState();

                shader = Source.shader;
                drawSize = Source.DrawSize;
                relativeHeight = height / Source.DrawHeight;
            }

            public override void Draw(IRenderer renderer)
            {
                base.Draw(renderer);

                shader.Bind();
                shader.GetUniform<float>("relativeHeight").UpdateValue(ref relativeHeight);

                var drawQuad = new Quad(
                    Vector2Extensions.Transform(Vector2.Zero, DrawInfo.Matrix),
                    Vector2Extensions.Transform(new Vector2(drawSize.X, 0f), DrawInfo.Matrix),
                    Vector2Extensions.Transform(new Vector2(0f, drawSize.Y), DrawInfo.Matrix),
                    Vector2Extensions.Transform(drawSize, DrawInfo.Matrix)
                );

                renderer.DrawQuad(renderer.WhitePixel, drawQuad, DrawColourInfo.Colour);

                shader.Unbind();
            }
        }
    }
}
