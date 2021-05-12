using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace Mvis.Plugin.RulesetPanel.Objects.MusicVisualizers
{
    public class DotsMusicVisualizerDrawable : MusicVisualizerDrawable
    {
        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Texture = textures.Get("particle");
        }

        protected override VisualizerDrawNode CreateVisualizerDrawNode() => new DotsVisualizerDrawNode(this);

        private class DotsVisualizerDrawNode : VisualizerDrawNode<DotsMusicVisualizerDrawable>
        {
            public DotsVisualizerDrawNode(DotsMusicVisualizerDrawable source)
                : base(source)
            {
            }

            protected override void DrawNode()
            {
                Vector2 inflation = DrawInfo.MatrixInverse.ExtractScale().Xy;
                var dotSize = new Vector2((float)BarWidth);

                if (AudioData != null)
                {
                    float spacing = DegreeValue / AudioData.Count;

                    for (int i = 0; i < AudioData.Count; i++)
                    {
                        float rotation = MathHelper.DegreesToRadians(i * spacing - 90);
                        float rotationCos = MathF.Cos(rotation);
                        float rotationSin = MathF.Sin(rotation);

                        var scale = (AudioData[i] * 2 + Size) / Size;
                        var multiplier = 1f / (scale * 2);

                        var dotPosition = new Vector2(rotationCos / 2 + multiplier, rotationSin / 2 + multiplier) * Size * scale;

                        var bottomOffset = new Vector2(-rotationSin * dotSize.X / 2, rotationCos * dotSize.Y / 2);
                        var amplitudeOffset = new Vector2(rotationCos * dotSize.X, rotationSin * dotSize.Y);

                        var rectangle = new Quad(
                            Vector2Extensions.Transform(dotPosition - bottomOffset, DrawInfo.Matrix),
                            Vector2Extensions.Transform(dotPosition - bottomOffset + amplitudeOffset, DrawInfo.Matrix),
                            Vector2Extensions.Transform(dotPosition + bottomOffset, DrawInfo.Matrix),
                            Vector2Extensions.Transform(dotPosition + bottomOffset + amplitudeOffset, DrawInfo.Matrix)
                        );

                        DrawQuad(
                            Texture,
                            rectangle,
                            DrawColourInfo.Colour,
                            null,
                            VertexBatch.AddAction,
                            Vector2.Divide(inflation, dotSize.Yx));
                    }
                }
            }
        }
    }
}
