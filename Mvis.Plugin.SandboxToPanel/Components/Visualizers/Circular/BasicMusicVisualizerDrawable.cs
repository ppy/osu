using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osuTK;

namespace Mvis.Plugin.Sandbox.Components.Visualizers.Circular
{
    public class BasicMusicVisualizerDrawable : CircularMusicVisualizerDrawable
    {
        protected override CircularVisualizerDrawNode CreateCircularVisualizerDrawNode() => new BasicVisualizerDrawNode(this);

        private class BasicVisualizerDrawNode : CircularVisualizerDrawNode
        {
            public BasicVisualizerDrawNode(BasicMusicVisualizerDrawable source)
                : base(source)
            {
            }

            protected override void DrawBar(int index, float data, float spacing, Vector2 inflation)
            {
                float rotation = MathHelper.DegreesToRadians(index * spacing - 90);
                float rotationCos = MathF.Cos(rotation);
                float rotationSin = MathF.Sin(rotation);

                var barPosition = new Vector2(rotationCos / 2 + 0.5f, rotationSin / 2 + 0.5f) * Size.X;
                var barSize = new Vector2((float)BarWidth, 2 + data);

                var bottomOffset = new Vector2(-rotationSin * barSize.X / 2, rotationCos * barSize.X / 2);
                var amplitudeOffset = new Vector2(rotationCos * barSize.Y, rotationSin * barSize.Y);

                var rectangle = new Quad(
                        Vector2Extensions.Transform(barPosition - bottomOffset, DrawInfo.Matrix),
                        Vector2Extensions.Transform(barPosition - bottomOffset + amplitudeOffset, DrawInfo.Matrix),
                        Vector2Extensions.Transform(barPosition + bottomOffset, DrawInfo.Matrix),
                        Vector2Extensions.Transform(barPosition + bottomOffset + amplitudeOffset, DrawInfo.Matrix)
                    );

                DrawQuad(
                    Texture,
                    rectangle,
                    DrawColourInfo.Colour,
                    null,
                    VertexBatch.AddAction,
                    Vector2.Divide(inflation, barSize.Yx));
            }
        }
    }
}
