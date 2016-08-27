//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK.Graphics.ES20;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using OpenTK.Graphics;
using OpenTK;

namespace osu.Framework.Graphics.OpenGL
{
    public static class Vertex
    {
        private static int amountEnabledAttributes = 0;
        public static void EnableAttributes(int amount)
        {
            if (amount == amountEnabledAttributes)
                return;
            else if (amount > amountEnabledAttributes)
            {
                for (int i = amountEnabledAttributes; i < amount; ++i)
                {
                    GL.EnableVertexAttribArray(i);
                }
            }
            else
            {
                for (int i = amountEnabledAttributes - 1; i >= amount; --i)
                {
                    GL.DisableVertexAttribArray(i);
                }
            }

            amountEnabledAttributes = amount;
        }
    }

    
    [StructLayout(LayoutKind.Sequential)]
    public struct UncolouredVertex2d : IEquatable<UncolouredVertex2d>
    {
        public Vector2 Position;

        private static readonly IntPtr positionOffset = Marshal.OffsetOf(typeof(UncolouredVertex2d), "Position");

        public bool Equals(UncolouredVertex2d other)
        {
            return Position.Equals(other.Position);
        }

        public static void Bind()
        {
            Vertex.EnableAttributes(1);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Stride, positionOffset);
        }

        public static readonly int Stride = OpenTK.BlittableValueType.StrideOf(new UncolouredVertex2d());
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex2d : IEquatable<Vertex2d>
    {
        public Vector2 Position;
        public Color4 Colour;

        private static readonly IntPtr positionOffset = Marshal.OffsetOf(typeof(Vertex2d), "Position");
        private static readonly IntPtr colourOffset = Marshal.OffsetOf(typeof(Vertex2d), "Colour");

        public bool Equals(Vertex2d other)
        {
            return Position.Equals(other.Position) && Colour.Equals(other.Colour);
        }

        public static void Bind()
        {
            Vertex.EnableAttributes(2);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Stride, positionOffset);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, Stride, colourOffset);
        }

        public static readonly int Stride = OpenTK.BlittableValueType.StrideOf(new Vertex2d());
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TexturedVertex2d : IEquatable<TexturedVertex2d>
    {
        public Vector2 Position;
        public Color4 Colour;
        public Vector2 TexturePosition;

        private static readonly IntPtr positionOffset = Marshal.OffsetOf(typeof(TexturedVertex2d), "Position");
        private static readonly IntPtr colourOffset = Marshal.OffsetOf(typeof(TexturedVertex2d), "Colour");
        private static readonly IntPtr texturePositionOffset = Marshal.OffsetOf(typeof(TexturedVertex2d), "TexturePosition");

        public bool Equals(TexturedVertex2d other)
        {
            return Position.Equals(other.Position) && TexturePosition.Equals(other.TexturePosition) && Colour.Equals(other.Colour);
        }

        public static void Bind()
        {
            Vertex.EnableAttributes(3);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Stride, positionOffset);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, Stride, colourOffset);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Stride, texturePositionOffset);
        }

        public static readonly int Stride = OpenTK.BlittableValueType.StrideOf(new TexturedVertex2d());
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TimedTexturedVertex2d : IEquatable<TimedTexturedVertex2d>
    {
        public Vector2 Position;
        public Color4 Colour;
        public Vector2 TexturePosition;
        public float Time;

        private static readonly IntPtr positionOffset = Marshal.OffsetOf(typeof(TimedTexturedVertex2d), "Position");
        private static readonly IntPtr colourOffset = Marshal.OffsetOf(typeof(TimedTexturedVertex2d), "Colour");
        private static readonly IntPtr texturePositionOffset = Marshal.OffsetOf(typeof(TimedTexturedVertex2d), "TexturePosition");
        private static readonly IntPtr timeOffset = Marshal.OffsetOf(typeof(TimedTexturedVertex2d), "Time");

        public bool Equals(TimedTexturedVertex2d other)
        {
            return Position.Equals(other.Position) && TexturePosition.Equals(other.TexturePosition) && Colour.Equals(other.Colour) && Time.Equals(other.Time);
        }

        public static void Bind()
        {
            Vertex.EnableAttributes(4);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Stride, positionOffset);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, Stride, colourOffset);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Stride, texturePositionOffset);
            GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, Stride, timeOffset);
        }

        public static readonly int Stride = OpenTK.BlittableValueType.StrideOf(new TimedTexturedVertex2d());
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ParticleVertex2d : IEquatable<ParticleVertex2d>
    {
        public Vector2 Position;
        public Color4 Colour;
        public Vector2 TexturePosition;
        public float Time;
        public Vector2 Direction;

        private static readonly IntPtr positionOffset = Marshal.OffsetOf(typeof(ParticleVertex2d), "Position");
        private static readonly IntPtr colourOffset = Marshal.OffsetOf(typeof(ParticleVertex2d), "Colour");
        private static readonly IntPtr texturePositionOffset = Marshal.OffsetOf(typeof(ParticleVertex2d), "TexturePosition");
        private static readonly IntPtr timeOffset = Marshal.OffsetOf(typeof(ParticleVertex2d), "Time");
        private static readonly IntPtr directionOffset = Marshal.OffsetOf(typeof(ParticleVertex2d), "Direction");

        public bool Equals(ParticleVertex2d other)
        {
            return Position.Equals(other.Position) && TexturePosition.Equals(other.TexturePosition) && Colour.Equals(other.Colour) && Time.Equals(other.Time) && Direction.Equals(other.Direction);
        }

        public static void Bind()
        {
            Vertex.EnableAttributes(5);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Stride, positionOffset);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, Stride, colourOffset);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Stride, texturePositionOffset);
            GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, Stride, timeOffset);
            GL.VertexAttribPointer(4, 2, VertexAttribPointerType.Float, false, Stride, directionOffset);
        }

        public static readonly int Stride = OpenTK.BlittableValueType.StrideOf(new ParticleVertex2d());
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TexturedVertex3d : IEquatable<TexturedVertex3d>
    {
        public OpenTK.Vector3 Position;
        public Color4 Colour;
        public Vector2 TexturePosition;

        private static readonly IntPtr positionOffset = Marshal.OffsetOf(typeof(TexturedVertex3d), "Position");
        private static readonly IntPtr colourOffset = Marshal.OffsetOf(typeof(TexturedVertex3d), "Colour");
        private static readonly IntPtr texturePositionOffset = Marshal.OffsetOf(typeof(TexturedVertex3d), "TexturePosition");

        public bool Equals(TexturedVertex3d other)
        {
            return Position.Equals(other.Position) && TexturePosition.Equals(other.TexturePosition) && Colour.Equals(other.Colour);
        }

        public static void Bind()
        {
            Vertex.EnableAttributes(3);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Stride, positionOffset);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, Stride, colourOffset);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Stride, texturePositionOffset);
        }

        public static readonly int Stride = OpenTK.BlittableValueType.StrideOf(new TexturedVertex3d());
    }
}
