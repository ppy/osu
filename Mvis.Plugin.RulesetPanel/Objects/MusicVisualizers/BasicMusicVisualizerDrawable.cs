using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osuTK;

namespace Mvis.Plugin.RulesetPanel.Objects.MusicVisualizers
{
    public class BasicMusicVisualizerDrawable : MusicVisualizerDrawable
    {
        protected override VisualizerDrawNode CreateVisualizerDrawNode() => new BasicVisualizerDrawNode(this);

        private class BasicVisualizerDrawNode : VisualizerDrawNode<BasicMusicVisualizerDrawable>
        {
            public BasicVisualizerDrawNode(BasicMusicVisualizerDrawable source)
                : base(source)
            {
            }

            protected override void DrawNode()
            {
                Vector2 inflation = DrawInfo.MatrixInverse.ExtractScale().Xy;

                if (AudioData != null)
                {
                    float spacing = DegreeValue / AudioData.Count;

                    for (int i = 0; i < AudioData.Count; i++)
                    {
                        float rotation = MathHelper.DegreesToRadians(i * spacing - 90);
                        float rotationCos = MathF.Cos(rotation);
                        float rotationSin = MathF.Sin(rotation);

                        // taking the cos and sin to the 0..1 range
                        var barPosition = new Vector2(rotationCos / 2 + 0.5f, rotationSin / 2 + 0.5f) * Size;

                        var barSize = new Vector2((float)BarWidth, 2 + AudioData[i]);

                        // The distance between the position and the sides of the bar.
                        var bottomOffset = new Vector2(-rotationSin * barSize.X / 2, rotationCos * barSize.X / 2);
                        // The distance between the bottom side of the bar and the top side.
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
                            // barSize by itself will make it smooth more in the X axis than in the Y axis, this reverts that.
                            Vector2.Divide(inflation, barSize.Yx));
                    }
                }
            }
        }
    }
}
