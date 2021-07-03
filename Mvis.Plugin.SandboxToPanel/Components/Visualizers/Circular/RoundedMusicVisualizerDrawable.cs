using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace Mvis.Plugin.Sandbox.Components.Visualizers.Circular
{
    public class RoundedMusicVisualizerDrawable : CircularMusicVisualizerDrawable
    {
        private Texture circleTexture;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            circleTexture = textures.Get("Visualizer/particle");
        }

        protected override CircularVisualizerDrawNode CreateCircularVisualizerDrawNode() => new RoundedVisualizerDrawNode(this);

        private class RoundedVisualizerDrawNode : CircularVisualizerDrawNode
        {
            protected new RoundedMusicVisualizerDrawable Source => (RoundedMusicVisualizerDrawable)base.Source;

            private Texture circleTexture;

            public RoundedVisualizerDrawNode(RoundedMusicVisualizerDrawable source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();
                circleTexture = Source.circleTexture;
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

                // Bottom Dot
                var bottomDotPosition = new Vector2(rotationCos / 2 + 0.5f, rotationSin / 2 + 0.5f) * Size.X;

                var dotBottomOffset = new Vector2(-rotationSin * dotSize.X / 2, rotationCos * dotSize.Y / 2);
                var dotAmplitudeOffset = new Vector2(rotationCos * dotSize.X, rotationSin * dotSize.Y);

                var bottomDotQuad = new Quad(
                        Vector2Extensions.Transform(bottomDotPosition - dotBottomOffset, DrawInfo.Matrix),
                        Vector2Extensions.Transform(bottomDotPosition - dotBottomOffset + dotAmplitudeOffset, DrawInfo.Matrix),
                        Vector2Extensions.Transform(bottomDotPosition + dotBottomOffset, DrawInfo.Matrix),
                        Vector2Extensions.Transform(bottomDotPosition + dotBottomOffset + dotAmplitudeOffset, DrawInfo.Matrix)
                    );

                DrawQuad(
                    circleTexture,
                    bottomDotQuad,
                    DrawColourInfo.Colour,
                    null,
                    VertexBatch.AddAction,
                    Vector2.Divide(inflation, dotSize.Yx));


                // Bar
                var barPositionScale = (dotSize.X + Size.X) / Size.X;
                var barPositionMultiplier = 1f / (barPositionScale * 2);

                var barPosition = new Vector2(rotationCos / 2 + barPositionMultiplier, rotationSin / 2 + barPositionMultiplier) * Size.X * barPositionScale;
                var barSize = new Vector2((float)BarWidth, data);

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


                // Top Dot
                var topDotScale = (barSize.Y * 2 + Size.X) / Size.X;
                var topDotPositionMultiplier = 1f / (topDotScale * 2);

                var topDotPosition = new Vector2(rotationCos / 2 + topDotPositionMultiplier, rotationSin / 2 + topDotPositionMultiplier) * Size.X * topDotScale;

                var topDotQuad = new Quad(
                        Vector2Extensions.Transform(topDotPosition - dotBottomOffset, DrawInfo.Matrix),
                        Vector2Extensions.Transform(topDotPosition - dotBottomOffset + dotAmplitudeOffset, DrawInfo.Matrix),
                        Vector2Extensions.Transform(topDotPosition + dotBottomOffset, DrawInfo.Matrix),
                        Vector2Extensions.Transform(topDotPosition + dotBottomOffset + dotAmplitudeOffset, DrawInfo.Matrix)
                    );

                DrawQuad(
                    circleTexture,
                    topDotQuad,
                    DrawColourInfo.Colour,
                    null,
                    VertexBatch.AddAction,
                    Vector2.Divide(inflation, dotSize.Yx));
            }
        }
    }
}
