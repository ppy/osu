using System;
using System.Collections.Generic;
using Mvis.Plugin.Sandbox.Config;
using Mvis.Plugin.Sandbox.Extensions;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osuTK;

namespace Mvis.Plugin.Sandbox.Components
{
    public class ParticlesDrawable : Sprite
    {
        private const float min_depth = 1f;
        private const float max_depth = 1000f;
        private const float particle_max_size = 4;
        private const float particle_min_size = 0.5f;
        private const float depth_speed_multiplier = 0.05f;
        private const float side_speed_multiplier = 0.0005f;

        public readonly Bindable<MoveDirection> Direction = new Bindable<MoveDirection>();

        [Resolved(canBeNull: true)]
        private SandboxConfigManager config { get; set; }

        private readonly Bindable<int> count = new Bindable<int>(1000);
        private readonly BindableBool isVisible = new BindableBool(true);

        private readonly List<Particle> parts = new List<Particle>();

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            RelativeSizeAxes = Axes.Both;
            Texture = textures.Get("Visualizer/particle");

            config?.BindWith(SandboxSetting.ParticleCount, count);
            config?.BindWith(SandboxSetting.ShowParticles, isVisible);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            isVisible.BindValueChanged(visible => Alpha = visible.NewValue ? 1 : 0, true);
            count.BindValueChanged(c => Restart(c.NewValue), true);
        }

        public void Restart(int particleCount)
        {
            parts.Clear();

            for (int i = 0; i < particleCount; i++)
                parts.Add(new Particle());
        }

        public void SetRandomDirection()
        {
            var count = Enum.GetValues(typeof(MoveDirection)).Length;
            var newDirection = (MoveDirection)RNG.Next(count);

            if (Direction.Value == newDirection)
            {
                SetRandomDirection();
                return;
            }

            Direction.Value = newDirection;
        }

        protected override void Update()
        {
            base.Update();

            var timeDiff = (float)Clock.ElapsedFrameTime * depth_speed_multiplier;
            var multiplier = Math.Max(DrawSize.Y, DrawSize.X) / Math.Min(DrawSize.Y, DrawSize.X);
            bool horizontalIsFaster = DrawSize.Y >= DrawSize.X;

            foreach (var p in parts)
                p.UpdateCurrentPosition(timeDiff, Direction.Value, multiplier, horizontalIsFaster);

            Invalidate(Invalidation.DrawNode);
        }

        protected override DrawNode CreateDrawNode() => new ParticleDrawNode(this);

        protected override void Dispose(bool isDisposing)
        {
            parts.Clear();
            base.Dispose(isDisposing);
        }

        private class ParticleDrawNode : SpriteDrawNode
        {
            private readonly List<Particle> parts = new List<Particle>();

            private ParticlesDrawable source => (ParticlesDrawable)Source;

            private Vector2 sourceSize;

            public ParticleDrawNode(Sprite source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                parts.Clear();
                parts.AddRange(source.parts);

                sourceSize = source.DrawSize;
            }

            protected override void Blit(Action<TexturedVertex2D> vertexAction)
            {
                foreach (var p in parts)
                {
                    var rect = getPartRectangle(p.CurrentPosition, p.CurrentSize);
                    var quad = getQuad(rect);

                    drawPart(quad, p.CurrentAlpha, vertexAction);
                }
            }

            private void drawPart(Quad quad, float alpha, Action<TexturedVertex2D> vertexAction)
            {
                DrawQuad(Texture, quad, DrawColourInfo.Colour.MultiplyAlpha(alpha), null, vertexAction,
                        new Vector2(InflationAmount.X / DrawRectangle.Width, InflationAmount.Y / DrawRectangle.Height),
                        null, TextureCoords);
            }

            private Quad getQuad(RectangleF rect) => new Quad(
                        Vector2Extensions.Transform(rect.TopLeft, DrawInfo.Matrix),
                        Vector2Extensions.Transform(rect.TopRight, DrawInfo.Matrix),
                        Vector2Extensions.Transform(rect.BottomLeft, DrawInfo.Matrix),
                        Vector2Extensions.Transform(rect.BottomRight, DrawInfo.Matrix)
                    );

            private RectangleF getPartRectangle(Vector2 pos, float size) => new RectangleF(
                        pos.X * sourceSize.X + sourceSize.X / 2 - size / 2,
                        pos.Y * sourceSize.Y + sourceSize.Y / 2 - size / 2,
                        size,
                        size);
        }

        private class Particle
        {
            /// <summary>
            /// Position at the minimum depth
            /// </summary>
            private Vector2? initialPosition;

            private float currentDepth;

            public Vector2 CurrentPosition { get; private set; }

            public float CurrentSize { get; private set; }

            public float CurrentAlpha { get; private set; }

            public bool Backwards { get; set; }

            public Particle()
            {
                reset(MoveDirection.Forward, false);
            }

            private void reset(MoveDirection direction, bool maxDepth = true)
            {
                switch (direction)
                {
                    default:
                    case MoveDirection.Backwards:
                        initialPosition = new Vector2(RNG.NextSingle(-0.5f, 0.5f), RNG.NextSingle(-0.5f, 0.5f)) * max_depth;
                        CurrentPosition = getPositionOnTheEdge(Vector2.Divide(initialPosition.Value, max_depth));
                        currentDepth = initialPosition.Value.X / CurrentPosition.X;
                        break;

                    case MoveDirection.Forward:
                        currentDepth = maxDepth ? max_depth : RNG.NextSingle(min_depth, max_depth);
                        initialPosition = new Vector2(RNG.NextSingle(-0.5f, 0.5f), RNG.NextSingle(-0.5f, 0.5f)) * max_depth;
                        CurrentPosition = getCurrentPosition(direction);
                        if (outOfBounds)
                        {
                            reset(direction, maxDepth);
                            return;
                        }
                        break;

                    case MoveDirection.Left:
                        CurrentPosition = getRandomPositionAtTheLeft();
                        break;

                    case MoveDirection.Right:
                        CurrentPosition = getRandomPositionAtTheRight();
                        break;

                    case MoveDirection.Up:
                        CurrentPosition = getRandomPositionAtTheTop();
                        break;

                    case MoveDirection.Down:
                        CurrentPosition = getRandomPositionAtTheBottom();
                        break;
                }

                updateProperties();
            }

            public void UpdateCurrentPosition(float timeDifference, MoveDirection direction, float multiplier, bool horizontalIsFaster)
            {
                switch (direction)
                {
                    default:
                    case MoveDirection.Forward:
                        currentDepth -= timeDifference;

                        if (currentDepth < min_depth)
                        {
                            reset(direction);
                            return;
                        }

                        CurrentPosition = getCurrentPosition(direction);

                        if (outOfBounds)
                        {
                            reset(direction);
                            return;
                        }

                        break;

                    case MoveDirection.Backwards:
                        currentDepth += timeDifference;

                        if (currentDepth > max_depth)
                        {
                            reset(direction);
                            return;
                        }

                        CurrentPosition = getCurrentPosition(direction);
                        break;

                    case MoveDirection.Left:
                        initialPosition = null;
                        CurrentPosition += new Vector2(max_depth / currentDepth * timeDifference * (horizontalIsFaster ? multiplier : 1) * side_speed_multiplier, 0);

                        if (CurrentPosition.X > 0.5f)
                        {
                            reset(direction);
                        }
                        break;

                    case MoveDirection.Right:
                        initialPosition = null;
                        CurrentPosition -= new Vector2(max_depth / currentDepth * timeDifference * (horizontalIsFaster ? multiplier : 1) * side_speed_multiplier, 0);

                        if (CurrentPosition.X < -0.5f)
                        {
                            reset(direction);
                        }
                        break;

                    case MoveDirection.Up:
                        initialPosition = null;
                        CurrentPosition += new Vector2(0, max_depth / currentDepth * timeDifference * (horizontalIsFaster ? 1 : multiplier) * side_speed_multiplier);

                        if (CurrentPosition.Y > 0.5f)
                        {
                            reset(direction);
                        }
                        break;

                    case MoveDirection.Down:
                        initialPosition = null;
                        CurrentPosition -= new Vector2(0, max_depth / currentDepth * timeDifference * (horizontalIsFaster ? 1 : multiplier) * side_speed_multiplier);

                        if (CurrentPosition.Y < -0.5f)
                        {
                            reset(direction);
                        }

                        break;
                }

                updateProperties();
            }

            private void updateProperties()
            {
                CurrentSize = MathExtensions.Map(currentDepth, max_depth, min_depth, particle_min_size, particle_max_size);
                CurrentAlpha = CurrentSize < 1 ? MathExtensions.Map(CurrentSize, particle_min_size, 1, 0, 1) : 1;
            }

            private Vector2 getCurrentPosition(MoveDirection direction)
            {
                switch (direction)
                {
                    default:
                    case MoveDirection.Forward:
                    case MoveDirection.Backwards:
                        if (initialPosition.HasValue)
                        {
                            return Vector2.Divide(initialPosition.Value, currentDepth);
                        }
                        else
                        {
                            initialPosition = CurrentPosition * currentDepth;
                            return Vector2.Divide(initialPosition.Value, currentDepth);
                        }
                }
            }

            private static Vector2 getPositionOnTheEdge(Vector2 inBoundsPosition)
            {
                float x;
                float y;
                float ratio;

                if (Math.Abs(inBoundsPosition.X) > Math.Abs(inBoundsPosition.Y))
                {
                    ratio = Math.Abs(inBoundsPosition.X) / 0.5f;
                    x = inBoundsPosition.X > 0 ? 0.5f : -0.5f;
                    y = inBoundsPosition.Y / ratio;
                }
                else
                {
                    ratio = Math.Abs(inBoundsPosition.Y) / 0.5f;
                    y = inBoundsPosition.Y > 0 ? 0.5f : -0.5f;
                    x = inBoundsPosition.X / ratio;
                }

                return new Vector2(x, y);
            }

            private static Vector2 getRandomPositionAtTheTop() => new Vector2(RNG.NextSingle(-0.5f, 0.5f), -0.5f);
            private static Vector2 getRandomPositionAtTheBottom() => new Vector2(RNG.NextSingle(-0.5f, 0.5f), 0.5f);
            private static Vector2 getRandomPositionAtTheLeft() => new Vector2(-0.5f, RNG.NextSingle(-0.5f, 0.5f));
            private static Vector2 getRandomPositionAtTheRight() => new Vector2(0.5f, RNG.NextSingle(-0.5f, 0.5f));

            private bool outOfBounds => CurrentPosition.X > 0.5f || CurrentPosition.X < -0.5f || CurrentPosition.Y > 0.5f || CurrentPosition.Y < -0.5f;
        }
    }

    public enum MoveDirection
    {
        Forward,
        Backwards,
        Left,
        Right,
        Up,
        Down
    }
}
