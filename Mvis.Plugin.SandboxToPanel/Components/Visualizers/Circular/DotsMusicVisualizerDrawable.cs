using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace Mvis.Plugin.RulesetPanel.Components.Visualizers.Circular
{
    public class DotsMusicVisualizerDrawable : CircularMusicVisualizerDrawable
    {
        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Texture = textures.Get("Visualizer/particle");
        }

        protected override CircularVisualizerDrawNode CreateCircularVisualizerDrawNode() => new DotsVisualizerDrawNode(this);

        private class DotsVisualizerDrawNode : CircularVisualizerDrawNode
        {
            public DotsVisualizerDrawNode(DotsMusicVisualizerDrawable source)
                : base(source)
            {
            }

            private Vector2 dotSize;

            protected override void PreCompute()
            {
                base.PreCompute();
                dotSize = new Vector2((float)BarWidth);
            }

            protected override void DrawBar(int index, float data, float spacing, Vector2 inflation)
            {
                float rotation = MathHelper.DegreesToRadians(index * spacing - 90);
                float rotationCos = MathF.Cos(rotation);
                float rotationSin = MathF.Sin(rotation);

                var scale = (data * 2 + Size.X) / Size.X;
                var multiplier = 1f / (scale * 2);

                var dotPosition = new Vector2(rotationCos / 2 + multiplier, rotationSin / 2 + multiplier) * Size.X * scale;

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
                    VertexBatch.AddAction);
            }
        }
    }
}
