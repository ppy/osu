using System;
using System.Collections.Generic;
using Mvis.Plugin.RulesetPanel.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osuTK;

namespace Mvis.Plugin.RulesetPanel.Components.Visualizers.Circular
{
    public class FallMusicVisualizerDrawable : CircularMusicVisualizerDrawable
    {
        private float[] currentRawFallAudioData;
        private float[] maxFallBarValues;
        private float[] smoothFallAudioData;

        protected override void ResetArrays()
        {
            base.ResetArrays();

            currentRawFallAudioData = new float[AdjustedBarCount];
            maxFallBarValues = new float[AdjustedBarCount];
            smoothFallAudioData = new float[AdjustedBarCount];
        }

        protected override void ApplyData(int index, float newRawAudioDataAtIndex)
        {
            base.ApplyData(index, newRawAudioDataAtIndex);

            if (newRawAudioDataAtIndex > currentRawFallAudioData[index])
            {
                currentRawFallAudioData[index] = newRawAudioDataAtIndex;
                maxFallBarValues[index] = currentRawFallAudioData[index];
            }
        }

        protected override void UpdateData(int index, float timeDifference)
        {
            base.UpdateData(index, timeDifference);

            currentRawFallAudioData[index] -= maxFallBarValues[index] / (Decay.Value * 4) * timeDifference;
            smoothFallAudioData[index] = currentRawFallAudioData[index] * HeightMultiplier.Value;
        }

        protected override void PostUpdate()
        {
            base.PostUpdate();

            if (Smoothness.Value > 0)
                smoothFallAudioData.Smooth(Math.Min(Smoothness.Value, AdjustedBarCount / 2));
        }

        protected override CircularVisualizerDrawNode CreateCircularVisualizerDrawNode() => new FallVisualizerDrawNode(this);

        private class FallVisualizerDrawNode : CircularVisualizerDrawNode
        {
            protected new FallMusicVisualizerDrawable Source => (FallMusicVisualizerDrawable)base.Source;

            public FallVisualizerDrawNode(FallMusicVisualizerDrawable source)
                : base(source)
            {
            }

            private readonly List<float> fallAudioData = new List<float>();

            public override void ApplyState()
            {
                base.ApplyState();

                fallAudioData.Clear();
                fallAudioData.AddRange(Source.smoothFallAudioData);
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

                // Fall bar

                var fallBarData = Reversed ? fallAudioData[fallAudioData.Count - 1 - index] : fallAudioData[index];

                var scale = (fallBarData * 2 + Size.X) / Size.X;
                var multiplier = 1f / (scale * 2);

                var fallBarPosition = new Vector2(rotationCos / 2 + multiplier, rotationSin / 2 + multiplier) * Size.X * scale;
                var fallBarSize = new Vector2((float)BarWidth, 2);

                var fallBarAmplitudeOffset = new Vector2(rotationCos * fallBarSize.Y, rotationSin * fallBarSize.Y);

                var fallBarRectangle = new Quad(
                        Vector2Extensions.Transform(fallBarPosition - bottomOffset, DrawInfo.Matrix),
                        Vector2Extensions.Transform(fallBarPosition - bottomOffset + fallBarAmplitudeOffset, DrawInfo.Matrix),
                        Vector2Extensions.Transform(fallBarPosition + bottomOffset, DrawInfo.Matrix),
                        Vector2Extensions.Transform(fallBarPosition + bottomOffset + fallBarAmplitudeOffset, DrawInfo.Matrix)
                    );

                DrawQuad(
                    Texture,
                    fallBarRectangle,
                    DrawColourInfo.Colour,
                    null,
                    VertexBatch.AddAction,
                    Vector2.Divide(inflation, fallBarSize.Yx));
            }
        }
    }
}
