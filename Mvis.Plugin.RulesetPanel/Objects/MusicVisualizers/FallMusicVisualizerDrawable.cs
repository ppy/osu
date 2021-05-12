using System;
using System.Collections.Generic;
using Mvis.Plugin.RulesetPanel.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osuTK;

namespace Mvis.Plugin.RulesetPanel.Objects.MusicVisualizers
{
    public class FallMusicVisualizerDrawable : MusicVisualizerDrawable
    {
        private float[] currentRawFallAudioData;
        private float[] maxFallBarValues;
        private float[] smoothFallAudioData;

        protected override void ResetArrays(int barCount)
        {
            base.ResetArrays(barCount);

            currentRawFallAudioData = new float[barCount];
            maxFallBarValues = new float[barCount];
            smoothFallAudioData = new float[barCount];
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

        protected override void UpdateData(int index, float timeDifference, bool reversed)
        {
            base.UpdateData(index, timeDifference, reversed);

            currentRawFallAudioData[index] -= maxFallBarValues[index] / (Decay.Value * 4) * timeDifference;
            smoothFallAudioData[reversed ? BarCount.Value - index - 1 : index] = currentRawFallAudioData[index] * HeightMultiplier.Value;
        }

        protected override void PostUpdate()
        {
            base.PostUpdate();

            smoothFallAudioData.Smooth(Math.Max((int)Math.Round(BarCount.Value * 0.003f * 360f / DegreeValue.Value), 1));
        }

        protected override VisualizerDrawNode CreateVisualizerDrawNode() => new FallVisualizerDrawNode(this);

        private class FallVisualizerDrawNode : VisualizerDrawNode<FallMusicVisualizerDrawable>
        {
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

                        var barPosition = new Vector2(rotationCos / 2 + 0.5f, rotationSin / 2 + 0.5f) * Size;
                        var barSize = new Vector2((float)BarWidth, 2 + AudioData[i]);

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

                        var scale = (fallAudioData[i] * 2 + Size) / Size;
                        var multiplier = 1f / (scale * 2);

                        var fallBarPosition = new Vector2(rotationCos / 2 + multiplier, rotationSin / 2 + multiplier) * Size * scale;
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
    }
}
