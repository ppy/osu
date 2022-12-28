using System;
using System.Collections.Generic;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Configuration;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Extensions;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osuTK;

#nullable disable

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components
{
    public partial class ParticlesDrawable : Sprite
    {
        private const float min_depth = 1f;
        private const float max_depth = 1000f;
        private const float particle_max_size = 4;
        private const float particle_min_size = 0.5f;
        private const float depth_speed_multiplier = 0.05f;
        private const float side_speed_multiplier = 0.0005f;

        public readonly Bindable<ParticlesDirection> Direction = new Bindable<ParticlesDirection>();

        [Resolved(canBeNull: true)]
        private SandboxRulesetConfigManager config { get; set; }

        private readonly Bindable<int> count = new Bindable<int>(1000);
        private readonly Bindable<int> globalSpeed = new Bindable<int>(100);
        private readonly BindableBool isVisible = new BindableBool(true);

        private readonly List<Particle> parts = new List<Particle>();

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            RelativeSizeAxes = Axes.Both;
            Texture = textures.Get("Visualizer/particle");

            config?.BindWith(SandboxRulesetSetting.ParticleCount, count);
            config?.BindWith(SandboxRulesetSetting.ShowParticles, isVisible);
            config?.BindWith(SandboxRulesetSetting.GlobalSpeed, globalSpeed);
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

        protected override void Update()
        {
            base.Update();

            var currentTime = Clock.CurrentTime;
            var timeDiff = (float)Clock.ElapsedFrameTime * depth_speed_multiplier;
            var multiplier = Math.Max(DrawSize.Y, DrawSize.X) / Math.Min(DrawSize.Y, DrawSize.X);
            bool horizontalIsFaster = DrawSize.Y >= DrawSize.X;

            foreach (var p in parts)
                p.UpdateCurrentPosition(currentTime, timeDiff, Direction.Value, multiplier, horizontalIsFaster, globalSpeed.Value);

            Invalidate(Invalidation.DrawNode);
        }

        protected override DrawNode CreateDrawNode() => new ParticleDrawNode(this);

        protected override void Dispose(bool isDisposing)
        {
            parts.Clear();
            base.Dispose(isDisposing);
        }

        private partial class ParticleDrawNode : SpriteDrawNode
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

            protected override void Blit(IRenderer renderer)
            {
                foreach (var p in parts)
                {
                    var rect = getPartRectangle(p.CurrentPosition, p.CurrentSize);
                    var quad = getQuad(rect);

                    drawPart(quad, p.CurrentAlpha, renderer);
                }
            }

            private void drawPart(Quad quad, float alpha, IRenderer renderer)
            {
                renderer.DrawQuad(Texture, quad, DrawColourInfo.Colour.MultiplyAlpha(alpha), null,
                        inflationPercentage: new Vector2(InflationAmount.X / DrawRectangle.Width, InflationAmount.Y / DrawRectangle.Height),
                        textureCoords: TextureCoords);
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

        private partial class Particle
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

            private OpenSimplexNoise noise;
            private int seed;

            public Particle()
            {
                reset(ParticlesDirection.Forward, false);
            }

            private void reset(ParticlesDirection direction, bool maxDepth = true)
            {
                seed = RNG.Next();
                noise = new OpenSimplexNoise(seed);

                switch (direction)
                {
                    default:
                    case ParticlesDirection.Backwards:
                        initialPosition = new Vector2(RNG.NextSingle(-0.5f, 0.5f), RNG.NextSingle(-0.5f, 0.5f)) * max_depth;
                        CurrentPosition = getPositionOnTheEdge(Vector2.Divide(initialPosition.Value, max_depth));
                        currentDepth = initialPosition.Value.X / CurrentPosition.X;
                        break;

                    case ParticlesDirection.Forward:
                        currentDepth = maxDepth ? max_depth : RNG.NextSingle(min_depth, max_depth);
                        initialPosition = new Vector2(RNG.NextSingle(-0.5f, 0.5f), RNG.NextSingle(-0.5f, 0.5f)) * max_depth;
                        CurrentPosition = getCurrentPosition(direction);
                        if (outOfBounds)
                        {
                            reset(direction, maxDepth);
                            return;
                        }
                        break;

                    case ParticlesDirection.Left:
                        CurrentPosition = getRandomPositionAtTheLeft();
                        break;

                    case ParticlesDirection.Right:
                        CurrentPosition = getRandomPositionAtTheRight();
                        break;

                    case ParticlesDirection.Up:
                        CurrentPosition = getRandomPositionAtTheTop();
                        break;

                    case ParticlesDirection.Down:
                        CurrentPosition = getRandomPositionAtTheBottom();
                        break;
                }

                updateProperties();
            }

            public void UpdateCurrentPosition(double time, float timeDifference, ParticlesDirection direction, float multiplier, bool horizontalIsFaster, int globalSpeed)
            {
                float globalSpeedMultiplier = globalSpeed / 100f;
                float baseComponent = max_depth / currentDepth * timeDifference * globalSpeedMultiplier;

                switch (direction)
                {
                    default:
                    case ParticlesDirection.Forward:
                        currentDepth -= timeDifference * globalSpeedMultiplier;

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

                    case ParticlesDirection.Backwards:
                        currentDepth += timeDifference * globalSpeedMultiplier;

                        if (currentDepth > max_depth)
                        {
                            reset(direction);
                            return;
                        }

                        CurrentPosition = getCurrentPosition(direction);
                        break;

                    case ParticlesDirection.Left:
                        initialPosition = null;
                        CurrentPosition += new Vector2(baseComponent * (horizontalIsFaster ? multiplier : 1) * side_speed_multiplier, 0);

                        if (outOfBounds)
                        {
                            reset(direction);
                            return;
                        }
                        break;

                    case ParticlesDirection.Random:
                        initialPosition = null;

                        Vector2 offset = new Vector2((float)noise.Evaluate(((float)time + seed) / 1000f, 0), (float)noise.Evaluate(((float)time + seed) / 1000f, 100));

                        CurrentPosition += new Vector2(baseComponent * (horizontalIsFaster ? multiplier : 1) * offset.X / 5000, baseComponent * (horizontalIsFaster ? 1 : multiplier) * offset.Y / 5000);

                        if (outOfBounds)
                        {
                            reset(direction);
                            return;
                        }
                        break;

                    case ParticlesDirection.Right:
                        initialPosition = null;
                        CurrentPosition -= new Vector2(baseComponent * (horizontalIsFaster ? multiplier : 1) * side_speed_multiplier, 0);

                        if (outOfBounds)
                        {
                            reset(direction);
                            return;
                        }
                        break;

                    case ParticlesDirection.Up:
                        initialPosition = null;
                        CurrentPosition += new Vector2(0, baseComponent * (horizontalIsFaster ? 1 : multiplier) * side_speed_multiplier);

                        if (outOfBounds)
                        {
                            reset(direction);
                            return;
                        }
                        break;

                    case ParticlesDirection.Down:
                        initialPosition = null;
                        CurrentPosition -= new Vector2(0, baseComponent * (horizontalIsFaster ? 1 : multiplier) * side_speed_multiplier);

                        if (outOfBounds)
                        {
                            reset(direction);
                            return;
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

            private Vector2 getCurrentPosition(ParticlesDirection direction)
            {
                switch (direction)
                {
                    default:
                    case ParticlesDirection.Forward:
                    case ParticlesDirection.Backwards:
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

    public enum ParticlesDirection
    {
        Random,
        Forward,
        Backwards,
        Left,
        Right,
        Up,
        Down
    }
}
