// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osuTK;
using osuTK.Graphics;
using osuTK.Graphics.ES30;

namespace osu.Game.Rulesets.Osu.UI.Cursor
{
    public class CursorTrail : Drawable, IRequireHighFrequencyMousePosition
    {
        private const int max_sprites = 2048;

        private readonly TrailPart[] parts = new TrailPart[max_sprites];
        private int currentIndex;
        private IShader shader;
        private double timeOffset;
        private float time;

        public CursorTrail()
        {
            // as we are currently very dependent on having a running clock, let's make our own clock for the time being.
            Clock = new FramedClock();

            RelativeSizeAxes = Axes.Both;

            for (int i = 0; i < max_sprites; i++)
            {
                // -1 signals that the part is unusable, and should not be drawn
                parts[i].InvalidationID = -1;
            }
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            shader = shaders.Load(@"CursorTrail", FragmentShaderDescriptor.TEXTURE);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            resetTime();
        }

        private Texture texture = Texture.WhitePixel;

        public Texture Texture
        {
            get => texture;
            set
            {
                if (texture == value)
                    return;

                texture = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        private readonly Cached<Vector2> partSizeCache = new Cached<Vector2>();

        private Vector2 partSize => partSizeCache.IsValid
            ? partSizeCache.Value
            : (partSizeCache.Value = new Vector2(Texture.DisplayWidth, Texture.DisplayHeight) * DrawInfo.Matrix.ExtractScale().Xy);

        public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
        {
            if ((invalidation & (Invalidation.DrawInfo | Invalidation.RequiredParentSizeToFit | Invalidation.Presence)) > 0)
                partSizeCache.Invalidate();

            return base.Invalidate(invalidation, source, shallPropagate);
        }

        /// <summary>
        /// The amount of time to fade the cursor trail pieces.
        /// </summary>
        protected virtual double FadeDuration => 300;

        public override bool IsPresent => true;

        protected override void Update()
        {
            base.Update();

            Invalidate(Invalidation.DrawNode, shallPropagate: false);

            const int fade_clock_reset_threshold = 1000000;

            time = (float)((Time.Current - timeOffset) / FadeDuration);
            if (time > fade_clock_reset_threshold)
                resetTime();
        }

        private void resetTime()
        {
            for (int i = 0; i < parts.Length; ++i)
            {
                parts[i].Time -= time;

                if (parts[i].InvalidationID != -1)
                    ++parts[i].InvalidationID;
            }

            time = 0;
            timeOffset = Time.Current;
        }

        /// <summary>
        /// Whether to interpolate mouse movements and add trail pieces at intermediate points.
        /// </summary>
        protected virtual bool InterpolateMovements => true;

        private Vector2? lastPosition;
        private readonly InputResampler resampler = new InputResampler();

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            Vector2 pos = e.ScreenSpaceMousePosition;

            if (lastPosition == null)
            {
                lastPosition = pos;
                resampler.AddPosition(lastPosition.Value);
                return base.OnMouseMove(e);
            }

            foreach (Vector2 pos2 in resampler.AddPosition(pos))
            {
                Trace.Assert(lastPosition.HasValue);

                if (InterpolateMovements)
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    Vector2 pos1 = lastPosition.Value;
                    Vector2 diff = pos2 - pos1;
                    float distance = diff.Length;
                    Vector2 direction = diff / distance;

                    float interval = partSize.X / 2.5f;

                    for (float d = interval; d < distance; d += interval)
                    {
                        lastPosition = pos1 + direction * d;
                        addPart(lastPosition.Value);
                    }
                }
                else
                {
                    lastPosition = pos2;
                    addPart(lastPosition.Value);
                }
            }

            return base.OnMouseMove(e);
        }

        private void addPart(Vector2 screenSpacePosition)
        {
            parts[currentIndex].Position = screenSpacePosition;
            parts[currentIndex].Time = time;
            ++parts[currentIndex].InvalidationID;

            currentIndex = (currentIndex + 1) % max_sprites;
        }

        protected override DrawNode CreateDrawNode() => new TrailDrawNode(this);

        private struct TrailPart
        {
            public Vector2 Position;
            public float Time;
            public long InvalidationID;
        }

        private class TrailDrawNode : DrawNode
        {
            protected new CursorTrail Source => (CursorTrail)base.Source;

            private IShader shader;
            private Texture texture;

            private float time;

            private readonly TrailPart[] parts = new TrailPart[max_sprites];
            private Vector2 size;

            private readonly TrailBatch vertexBatch = new TrailBatch(max_sprites, 1);

            public TrailDrawNode(CursorTrail source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                shader = Source.shader;
                texture = Source.texture;
                size = Source.partSize;
                time = Source.time;

                Source.parts.CopyTo(parts, 0);
            }

            public override void Draw(Action<TexturedVertex2D> vertexAction)
            {
                base.Draw(vertexAction);

                shader.Bind();
                shader.GetUniform<float>("g_FadeClock").UpdateValue(ref time);

                for (int i = 0; i < parts.Length; ++i)
                {
                    if (parts[i].InvalidationID == -1)
                        continue;

                    vertexBatch.DrawTime = parts[i].Time;

                    Vector2 pos = parts[i].Position;

                    DrawQuad(
                        texture,
                        new Quad(pos.X - size.X / 2, pos.Y - size.Y / 2, size.X, size.Y),
                        DrawColourInfo.Colour,
                        null,
                        vertexBatch.AddAction);
                }

                shader.Unbind();
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                vertexBatch.Dispose();
            }

            // Todo: This shouldn't exist, but is currently used to reduce allocations by caching variable-capturing closures.
            private class TrailBatch : QuadBatch<TexturedTrailVertex>
            {
                public new readonly Action<TexturedVertex2D> AddAction;
                public float DrawTime;

                public TrailBatch(int size, int maxBuffers)
                    : base(size, maxBuffers)
                {
                    AddAction = v => Add(new TexturedTrailVertex
                    {
                        Position = v.Position,
                        TexturePosition = v.TexturePosition,
                        Time = DrawTime + 1,
                        Colour = v.Colour,
                    });
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TexturedTrailVertex : IEquatable<TexturedTrailVertex>, IVertex
        {
            [VertexMember(2, VertexAttribPointerType.Float)]
            public Vector2 Position;

            [VertexMember(4, VertexAttribPointerType.Float)]
            public Color4 Colour;

            [VertexMember(2, VertexAttribPointerType.Float)]
            public Vector2 TexturePosition;

            [VertexMember(1, VertexAttribPointerType.Float)]
            public float Time;

            public bool Equals(TexturedTrailVertex other)
            {
                return Position.Equals(other.Position)
                       && TexturePosition.Equals(other.TexturePosition)
                       && Colour.Equals(other.Colour)
                       && Time.Equals(other.Time);
            }
        }
    }
}
