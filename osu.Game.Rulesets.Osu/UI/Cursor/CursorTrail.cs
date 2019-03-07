// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Buffers;
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
    internal class CursorTrail : Drawable, IRequireHighFrequencyMousePosition
    {
        private int currentIndex;

        private IShader shader;
        private Texture texture;

        private Vector2 size => texture.Size * Scale;

        private double timeOffset;

        private float time;

        public override bool IsPresent => true;

        private const int max_sprites = 2048;

        private readonly TrailPart[] parts = new TrailPart[max_sprites];

        private Vector2? lastPosition;

        private readonly InputResampler resampler = new InputResampler();

        protected override DrawNode CreateDrawNode() => new TrailDrawNode();

        protected override void ApplyDrawNode(DrawNode node)
        {
            base.ApplyDrawNode(node);

            TrailDrawNode tNode = (TrailDrawNode)node;
            tNode.Shader = shader;
            tNode.Texture = texture;
            tNode.Size = size;
            tNode.Time = time;

            for (int i = 0; i < parts.Length; ++i)
                if (parts[i].InvalidationID > tNode.Parts[i].InvalidationID)
                    tNode.Parts[i] = parts[i];
        }

        public CursorTrail()
        {
            // as we are currently very dependent on having a running clock, let's make our own clock for the time being.
            Clock = new FramedClock();

            RelativeSizeAxes = Axes.Both;

            for (int i = 0; i < max_sprites; i++)
            {
                parts[i].InvalidationID = 0;
                parts[i].WasUpdated = true;
            }
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders, TextureStore textures)
        {
            shader = shaders.Load(@"CursorTrail", FragmentShaderDescriptor.TEXTURE);
            texture = textures.Get(@"Cursor/cursortrail");
            Scale = new Vector2(1 / texture.ScaleAdjust);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            resetTime();
        }

        protected override void Update()
        {
            base.Update();

            Invalidate(Invalidation.DrawNode, shallPropagate: false);

            const int fade_clock_reset_threshold = 1000000;

            time = (float)(Time.Current - timeOffset) / 300f;
            if (time > fade_clock_reset_threshold)
                resetTime();
        }

        private void resetTime()
        {
            for (int i = 0; i < parts.Length; ++i)
            {
                parts[i].Time -= time;
                ++parts[i].InvalidationID;
            }

            time = 0;
            timeOffset = Time.Current;
        }

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

                // ReSharper disable once PossibleInvalidOperationException
                Vector2 pos1 = lastPosition.Value;
                Vector2 diff = pos2 - pos1;
                float distance = diff.Length;
                Vector2 direction = diff / distance;

                float interval = size.X / 2 * 0.9f;

                for (float d = interval; d < distance; d += interval)
                {
                    lastPosition = pos1 + direction * d;
                    addPosition(lastPosition.Value);
                }
            }

            return base.OnMouseMove(e);
        }

        private void addPosition(Vector2 pos)
        {
            parts[currentIndex].Position = pos;
            parts[currentIndex].Time = time;
            ++parts[currentIndex].InvalidationID;

            currentIndex = (currentIndex + 1) % max_sprites;
        }

        private struct TrailPart
        {
            public Vector2 Position;
            public float Time;
            public long InvalidationID;
            public bool WasUpdated;
        }

        private class TrailDrawNode : DrawNode
        {
            public IShader Shader;
            public Texture Texture;

            public float Time;

            public readonly TrailPart[] Parts = new TrailPart[max_sprites];
            public Vector2 Size;

            private readonly VertexBuffer<TexturedTrailVertex> vertexBuffer = new QuadVertexBuffer<TexturedTrailVertex>(max_sprites, BufferUsageHint.DynamicDraw);

            public TrailDrawNode()
            {
                for (int i = 0; i < max_sprites; i++)
                {
                    Parts[i].InvalidationID = 0;
                    Parts[i].WasUpdated = false;
                }
            }

            public override void Draw(Action<TexturedVertex2D> vertexAction)
            {
                Shader.GetUniform<float>("g_FadeClock").UpdateValue(ref Time);

                int updateStart = -1, updateEnd = 0;
                for (int i = 0; i < Parts.Length; ++i)
                {
                    if (Parts[i].WasUpdated)
                    {
                        if (updateStart == -1)
                            updateStart = i;
                        updateEnd = i + 1;

                        int start = i * 4;
                        int end = start;

                        Vector2 pos = Parts[i].Position;
                        float time = Parts[i].Time;

                        Texture.DrawQuad(
                            new Quad(pos.X - Size.X / 2, pos.Y - Size.Y / 2, Size.X, Size.Y),
                            DrawColourInfo.Colour,
                            null,
                            v => vertexBuffer.Vertices[end++] = new TexturedTrailVertex
                            {
                                Position = v.Position,
                                TexturePosition = v.TexturePosition,
                                Time = time + 1,
                                Colour = v.Colour,
                            });

                        Parts[i].WasUpdated = false;
                    }
                    else if (updateStart != -1)
                    {
                        vertexBuffer.UpdateRange(updateStart * 4, updateEnd * 4);
                        updateStart = -1;
                    }
                }

                // Update all remaining vertices that have been changed.
                if (updateStart != -1)
                    vertexBuffer.UpdateRange(updateStart * 4, updateEnd * 4);

                base.Draw(vertexAction);

                Shader.Bind();

                Texture.TextureGL.Bind();
                vertexBuffer.Draw();

                Shader.Unbind();
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                vertexBuffer.Dispose();
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
