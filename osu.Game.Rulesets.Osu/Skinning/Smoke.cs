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
using osu.Game.Rulesets.Osu.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public abstract class Smoke : Drawable, ITexturedShaderDrawable
    {
        public IShader? TextureShader { get; private set; }
        public IShader? RoundedTextureShader { get; private set; }

        private float? radius;

        protected float Radius
        {
            get => radius ?? Texture?.DisplayWidth * 0.165f ?? 3;
            set
            {
                if (radius == value)
                    return;

                radius = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        protected Texture? Texture { get; set; }

        protected double SmokeStartTime { get; private set; } = double.MinValue;

        protected double SmokeEndTime { get; private set; } = double.MaxValue;

        private Vector2 topLeft;

        protected Vector2 TopLeft
        {
            get => topLeft;
            set
            {
                if (topLeft == value)
                    return;

                topLeft = value;
                Invalidate(Invalidation.Layout);
            }
        }

        private Vector2 bottomRight;

        protected Vector2 BottomRight
        {
            get => bottomRight;
            set
            {
                if (bottomRight == value)
                    return;

                bottomRight = value;
                Invalidate(Invalidation.Layout);
            }
        }

        protected abstract double LifetimeAfterSmokeEnd { get; }
        protected virtual float PointInterval => Radius * 7f / 8;
        protected bool IsActive { get; private set; }

        protected readonly List<SmokePoint> SmokePoints = new List<SmokePoint>();

        private float totalDistance;
        private Vector2? lastPosition;

        private const int max_point_count = 18_000;

        public override float Height
        {
            get => base.Height = BottomRight.Y - TopLeft.Y;
            set => throw new InvalidOperationException($"Cannot manually set {nameof(Height)} of {nameof(Smoke)}.");
        }

        public override float Width
        {
            get => base.Width = BottomRight.X - TopLeft.X;
            set => throw new InvalidOperationException($"Cannot manually set {nameof(Width)} of {nameof(Smoke)}.");
        }

        public override Vector2 Size
        {
            get => base.Size = BottomRight - TopLeft;
            set => throw new InvalidOperationException($"Cannot manually set {nameof(Size)} of {nameof(Smoke)}.");
        }

        [Resolved(CanBeNull = true)]
        private SmokeContainer? smokeContainer { get; set; }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            RoundedTextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE_ROUNDED);
            TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Anchor = Anchor.TopLeft;
            Origin = Anchor.TopLeft;

            SmokeStartTime = Time.Current;

            totalDistance = PointInterval;

            if (smokeContainer != null)
            {
                smokeContainer.SmokeMoved += onSmokeMoved;
                smokeContainer.SmokeEnded += onSmokeEnded;
                IsActive = true;

                onSmokeMoved(smokeContainer.LastMousePosition, Time.Current);
            }
        }

        private Vector2 nextPointDirection()
        {
            float angle = RNG.NextSingle(0, 2 * MathF.PI);
            return new Vector2(MathF.Sin(angle), -MathF.Cos(angle));
        }

        private void onSmokeMoved(Vector2 position, double time)
        {
            if (!IsActive)
                return;

            lastPosition ??= position;

            float delta = (position - (Vector2)lastPosition).LengthFast;
            totalDistance += delta;
            int count = (int)(totalDistance / PointInterval);

            if (count > 0)
            {
                Vector2 increment = position - (Vector2)lastPosition;
                increment.NormalizeFast();

                Vector2 pointPos = (PointInterval - (totalDistance - delta)) * increment + (Vector2)lastPosition;
                increment *= PointInterval;

                if (SmokePoints.Count > 0 && SmokePoints[^1].Time > time)
                {
                    int index = ~SmokePoints.BinarySearch(new SmokePoint { Time = time }, new SmokePoint.UpperBoundComparer());
                    SmokePoints.RemoveRange(index, SmokePoints.Count - index);
                    recalculateBounds();
                }

                totalDistance %= PointInterval;

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
                adaptBounds(position);
            }

            lastPosition = position;

            if (SmokePoints.Count >= max_point_count)
                onSmokeEnded(time);
        }

        private void recalculateBounds()
        {
            TopLeft = BottomRight = Vector2.Zero;

            foreach (var point in SmokePoints)
                adaptBounds(point.Position);
        }

        private void adaptBounds(Vector2 position)
        {
            if (position.X < TopLeft.X)
                TopLeft = new Vector2(position.X, TopLeft.Y);
            else if (position.X > BottomRight.X)
                BottomRight = new Vector2(position.X, BottomRight.Y);

            if (position.Y < TopLeft.Y)
                TopLeft = new Vector2(TopLeft.X, position.Y);
            else if (position.Y > BottomRight.Y)
                BottomRight = new Vector2(BottomRight.X, position.Y);
        }

        private void onSmokeEnded(double time)
        {
            if (!IsActive)
                return;

            IsActive = false;
            SmokeEndTime = time;
            LifetimeEnd = time + LifetimeAfterSmokeEnd + 100;
        }

        protected abstract override DrawNode CreateDrawNode();

        protected override void Update()
        {
            base.Update();

            Position = TopLeft;

            Invalidate(Invalidation.DrawNode);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (smokeContainer != null)
            {
                smokeContainer.SmokeMoved -= onSmokeMoved;
                smokeContainer.SmokeEnded -= onSmokeEnded;
            }
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

        protected abstract class SmokeDrawNode : TexturedShaderDrawNode
        {
            protected new Smoke Source => (Smoke)base.Source;

            protected double SmokeStartTime { get; private set; }
            protected double SmokeEndTime { get; private set; }
            protected double CurrentTime { get; private set; }

            private readonly List<SmokePoint> points = new List<SmokePoint>();
            private IVertexBatch<TexturedVertex2D>? quadBatch;
            private float radius;
            private Vector2 drawSize;
            private Vector2 positionOffset;
            private Texture? texture;

            protected SmokeDrawNode(ITexturedShaderDrawable source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                points.Clear();
                points.AddRange(Source.SmokePoints);

                radius = Source.Radius;
                drawSize = Source.DrawSize;
                positionOffset = Source.TopLeft;
                texture = Source.Texture;

                SmokeStartTime = Source.SmokeStartTime;
                SmokeEndTime = Source.SmokeEndTime;
                CurrentTime = Source.Clock.CurrentTime;
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

            protected abstract Color4 PointColour(SmokePoint point);

            protected abstract float PointScale(SmokePoint point);

            protected abstract Vector2 PointDirection(SmokePoint point);

            private void drawPointQuad(SmokePoint point, RectangleF textureRect)
            {
                Debug.Assert(quadBatch != null);

                var colour = PointColour(point);
                float scale = PointScale(point);
                var dir = PointDirection(point);
                var ortho = dir.PerpendicularLeft;

                if (colour.A == 0 || scale == 0)
                    return;

                var localTopLeft = point.Position + (radius * scale * (-ortho - dir)) - positionOffset;
                var localTopRight = point.Position + (radius * scale * (-ortho + dir)) - positionOffset;
                var localBotLeft = point.Position + (radius * scale * (ortho - dir)) - positionOffset;
                var localBotRight = point.Position + (radius * scale * (ortho + dir)) - positionOffset;

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
