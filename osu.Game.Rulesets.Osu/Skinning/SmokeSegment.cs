// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Rendering.Vertices;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public abstract class SmokeSegment : Drawable, ITexturedShaderDrawable
    {
        private const int max_point_count = 18_000;

        // fade anim values
        private const double initial_fade_out_duration = 4000;

        private const double re_fade_in_speed = 3;
        private const double re_fade_in_duration = 50;

        private const double final_fade_out_speed = 2;
        private const double final_fade_out_duration = 8000;

        private const float initial_alpha = 0.6f;
        private const float re_fade_in_alpha = 1f;

        private readonly int rotationSeed = RNG.Next();

        // scale anim values
        private const double scale_duration = 1200;

        private const float initial_scale = 0.65f;
        private const float final_scale = 1f;

        // rotation anim values
        private const double rotation_duration = 500;

        private const float max_rotation = 0.25f;

        public IShader? TextureShader { get; private set; }
        public IShader? RoundedTextureShader { get; private set; }

        protected Texture? Texture { get; set; }

        private float radius => Texture?.DisplayWidth * 0.165f ?? 3;

        protected readonly List<SmokePoint> SmokePoints = new List<SmokePoint>();

        private float pointInterval => radius * 7f / 8;

        private double smokeStartTime { get; set; } = double.MinValue;

        private double smokeEndTime { get; set; } = double.MaxValue;

        private float totalDistance;
        private Vector2? lastPosition;

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            RoundedTextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE_ROUNDED);
            TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            RelativeSizeAxes = Axes.Both;

            LifetimeStart = smokeStartTime = Time.Current;

            totalDistance = pointInterval;
        }

        private Vector2 nextPointDirection()
        {
            float angle = RNG.NextSingle(0, 2 * MathF.PI);
            return new Vector2(MathF.Sin(angle), -MathF.Cos(angle));
        }

        public void AddPosition(Vector2 position, double time)
        {
            lastPosition ??= position;

            float delta = (position - (Vector2)lastPosition).LengthFast;
            totalDistance += delta;
            int count = (int)(totalDistance / pointInterval);

            if (count > 0)
            {
                Vector2 increment = position - (Vector2)lastPosition;
                increment.NormalizeFast();

                Vector2 pointPos = (pointInterval - (totalDistance - delta)) * increment + (Vector2)lastPosition;
                increment *= pointInterval;

                if (SmokePoints.Count > 0 && SmokePoints[^1].Time > time)
                {
                    int index = ~SmokePoints.BinarySearch(new SmokePoint { Time = time }, new SmokePoint.UpperBoundComparer());
                    SmokePoints.RemoveRange(index, SmokePoints.Count - index);
                }

                totalDistance %= pointInterval;

                for (int i = 0; i < count; i++)
                {
                    SmokePoints.Add(new SmokePoint
                    {
                        Position = pointPos,
                        Time = time,
                        Direction = nextPointDirection(),
                    });

                    pointPos += increment;
                }

                Invalidate(Invalidation.DrawNode);
            }

            lastPosition = position;

            if (SmokePoints.Count >= max_point_count)
                FinishDrawing(time);
        }

        public void FinishDrawing(double time)
        {
            smokeEndTime = time;

            double initialFadeOutDurationTrunc = Math.Min(initial_fade_out_duration, smokeEndTime - smokeStartTime);
            LifetimeEnd = smokeEndTime + final_fade_out_duration + initialFadeOutDurationTrunc / re_fade_in_speed + initialFadeOutDurationTrunc / final_fade_out_speed;
        }

        protected override DrawNode CreateDrawNode() => new SmokeDrawNode(this);

        protected override void Update()
        {
            base.Update();

            Invalidate(Invalidation.DrawNode);
        }

        protected struct SmokePoint
        {
            public Vector2 Position;
            public double Time;
            public Vector2 Direction;

            public struct UpperBoundComparer : IComparer<SmokePoint>
            {
                public int Compare(SmokePoint x, SmokePoint target)
                {
                    // By returning -1 when the target value is equal to x, guarantees that the
                    // element at BinarySearch's returned index will always be the first element
                    // larger. Since 0 is never returned, the target is never "found", so the return
                    // value will be the index's complement.

                    return x.Time > target.Time ? 1 : -1;
                }
            }
        }

        protected class SmokeDrawNode : TexturedShaderDrawNode
        {
            protected new SmokeSegment Source => (SmokeSegment)base.Source;

            protected double SmokeStartTime { get; private set; }
            protected double SmokeEndTime { get; private set; }
            protected double CurrentTime { get; private set; }

            private readonly List<SmokePoint> points = new List<SmokePoint>();
            private IVertexBatch<TexturedVertex2D>? quadBatch;
            private float radius;
            private Vector2 drawSize;
            private Texture? texture;

            // anim calculation vars (color, scale, direction)
            private double initialFadeOutDurationTrunc;
            private double firstVisiblePointTime;

            private double initialFadeOutTime;
            private double reFadeInTime;
            private double finalFadeOutTime;

            private Random rotationRNG = new Random();

            public SmokeDrawNode(ITexturedShaderDrawable source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                points.Clear();
                points.AddRange(Source.SmokePoints);

                radius = Source.radius;
                drawSize = Source.DrawSize;
                texture = Source.Texture;

                SmokeStartTime = Source.smokeStartTime;
                SmokeEndTime = Source.smokeEndTime;
                CurrentTime = Source.Clock.CurrentTime;

                rotationRNG = new Random(Source.rotationSeed);

                initialFadeOutDurationTrunc = Math.Min(initial_fade_out_duration, SmokeEndTime - SmokeStartTime);
                firstVisiblePointTime = SmokeEndTime - initialFadeOutDurationTrunc;

                initialFadeOutTime = CurrentTime;
                reFadeInTime = CurrentTime - initialFadeOutDurationTrunc - firstVisiblePointTime * (1 - 1 / re_fade_in_speed);
                finalFadeOutTime = CurrentTime - initialFadeOutDurationTrunc - firstVisiblePointTime * (1 - 1 / final_fade_out_speed);
            }

            public sealed override void Draw(IRenderer renderer)
            {
                base.Draw(renderer);

                if (points.Count == 0)
                    return;

                quadBatch ??= renderer.CreateQuadBatch<TexturedVertex2D>(max_point_count / 10, 10);
                texture ??= renderer.WhitePixel;
                RectangleF textureRect = texture.GetTextureRect();

                var shader = GetAppropriateShader(renderer);

                renderer.SetBlend(BlendingParameters.Additive);
                renderer.PushLocalMatrix(DrawInfo.Matrix);

                shader.Bind();
                texture.Bind();

                foreach (var point in points)
                    drawPointQuad(point, textureRect);

                shader.Unbind();
                renderer.PopLocalMatrix();
            }

            protected Color4 ColourAtPosition(Vector2 localPos) => DrawColourInfo.Colour.HasSingleColour
                ? ((SRGBColour)DrawColourInfo.Colour).Linear
                : DrawColourInfo.Colour.Interpolate(Vector2.Divide(localPos, drawSize)).Linear;

            protected virtual Color4 PointColour(SmokePoint point)
            {
                var color = Color4.White;

                double timeDoingInitialFadeOut = Math.Min(initialFadeOutTime, SmokeEndTime) - point.Time;

                if (timeDoingInitialFadeOut > 0)
                {
                    float fraction = Math.Clamp((float)(timeDoingInitialFadeOut / initial_fade_out_duration), 0, 1);
                    color.A = (1 - fraction) * initial_alpha;
                }

                if (color.A > 0)
                {
                    double timeDoingReFadeIn = reFadeInTime - point.Time / re_fade_in_speed;
                    double timeDoingFinalFadeOut = finalFadeOutTime - point.Time / final_fade_out_speed;

                    if (timeDoingFinalFadeOut > 0)
                    {
                        float fraction = Math.Clamp((float)(timeDoingFinalFadeOut / final_fade_out_duration), 0, 1);
                        fraction = MathF.Pow(fraction, 5);
                        color.A = (1 - fraction) * re_fade_in_alpha;
                    }
                    else if (timeDoingReFadeIn > 0)
                    {
                        float fraction = Math.Clamp((float)(timeDoingReFadeIn / re_fade_in_duration), 0, 1);
                        fraction = 1 - MathF.Pow(1 - fraction, 5);
                        color.A = fraction * (re_fade_in_alpha - color.A) + color.A;
                    }
                }

                return color;
            }

            protected virtual float PointScale(SmokePoint point)
            {
                double timeDoingScale = CurrentTime - point.Time;
                float fraction = Math.Clamp((float)(timeDoingScale / scale_duration), 0, 1);
                fraction = 1 - MathF.Pow(1 - fraction, 5);
                return fraction * (final_scale - initial_scale) + initial_scale;
            }

            protected virtual Vector2 PointDirection(SmokePoint point)
            {
                float initialAngle = MathF.Atan2(point.Direction.Y, point.Direction.X);
                float finalAngle = initialAngle + nextRotation();

                double timeDoingRotation = CurrentTime - point.Time;
                float fraction = Math.Clamp((float)(timeDoingRotation / rotation_duration), 0, 1);
                fraction = 1 - MathF.Pow(1 - fraction, 5);
                float angle = fraction * (finalAngle - initialAngle) + initialAngle;

                return new Vector2(MathF.Sin(angle), -MathF.Cos(angle));
            }

            private float nextRotation() => max_rotation * ((float)rotationRNG.NextDouble() * 2 - 1);

            private void drawPointQuad(SmokePoint point, RectangleF textureRect)
            {
                Debug.Assert(quadBatch != null);

                var colour = PointColour(point);
                float scale = PointScale(point);
                var dir = PointDirection(point);
                var ortho = dir.PerpendicularLeft;

                if (colour.A == 0 || scale == 0)
                    return;

                var localTopLeft = point.Position + (radius * scale * (-ortho - dir));
                var localTopRight = point.Position + (radius * scale * (-ortho + dir));
                var localBotLeft = point.Position + (radius * scale * (ortho - dir));
                var localBotRight = point.Position + (radius * scale * (ortho + dir));

                quadBatch.Add(new TexturedVertex2D
                {
                    Position = localTopLeft,
                    TexturePosition = textureRect.TopLeft,
                    Colour = Color4Extensions.Multiply(ColourAtPosition(localTopLeft), colour),
                });
                quadBatch.Add(new TexturedVertex2D
                {
                    Position = localTopRight,
                    TexturePosition = textureRect.TopRight,
                    Colour = Color4Extensions.Multiply(ColourAtPosition(localTopRight), colour),
                });
                quadBatch.Add(new TexturedVertex2D
                {
                    Position = localBotRight,
                    TexturePosition = textureRect.BottomRight,
                    Colour = Color4Extensions.Multiply(ColourAtPosition(localBotRight), colour),
                });
                quadBatch.Add(new TexturedVertex2D
                {
                    Position = localBotLeft,
                    TexturePosition = textureRect.BottomLeft,
                    Colour = Color4Extensions.Multiply(ColourAtPosition(localBotLeft), colour),
                });
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                quadBatch?.Dispose();
            }
        }
    }
}
