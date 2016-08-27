//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK.Graphics;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.Shaders;

namespace osu.Framework.Graphics.Drawables
{
    public class Box : Drawable
    {
        private QuadBatch<Vertex2d> quadBatch = new QuadBatch<Vertex2d>(1, 3);
        protected override IVertexBatch ActiveBatch => quadBatch;

        private static Shader shader;

        protected override void Draw()
        {
            base.Draw();

            if (shader == null)
                shader = Game.Shaders.Load(VertexShader.Colour, FragmentShader.Colour);

            shader.Bind();

            quadBatch.Add(new Vertex2d() { Colour = DrawInfo.Colour, Position = ScreenSpaceDrawQuad.BottomLeft });
            quadBatch.Add(new Vertex2d() { Colour = DrawInfo.Colour, Position = ScreenSpaceDrawQuad.BottomRight });
            quadBatch.Add(new Vertex2d() { Colour = DrawInfo.Colour, Position = ScreenSpaceDrawQuad.TopRight });
            quadBatch.Add(new Vertex2d() { Colour = DrawInfo.Colour, Position = ScreenSpaceDrawQuad.TopLeft });
            quadBatch.Draw();

            shader.Unbind();
        }
    }
}
