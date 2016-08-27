//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using OpenTK;
using osu.Framework.Extensions.PolygonExtensions;

namespace osu.Framework.Graphics.Primitives
{
    public struct Quad : IConvexPolygon
    {
        public Vector2 TopLeft;
        public Vector2 TopRight;
        public Vector2 BottomLeft;
        public Vector2 BottomRight;

        public Quad(Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
        }

        public Quad(float x, float y, float width, float height) : this()
        {
            TopLeft = new Vector2(x, y);
            TopRight = new Vector2(x + width, y);
            BottomLeft = new Vector2(x, y + height);
            BottomRight = new Vector2(x + width, y + height);
        }

        public static Quad FromRectangle(RectangleF rectangle)
        {
            return new Quad(new Vector2(rectangle.Left, rectangle.Top),
                                          new Vector2(rectangle.Right, rectangle.Top),
                                          new Vector2(rectangle.Left, rectangle.Bottom),
                                          new Vector2(rectangle.Right, rectangle.Bottom));
        }

        public static Quad FromRectangle(Rectangle rectangle)
        {
            return new Quad(new Vector2(rectangle.Left, rectangle.Top),
                                          new Vector2(rectangle.Right, rectangle.Top),
                                          new Vector2(rectangle.Left, rectangle.Bottom),
                                          new Vector2(rectangle.Right, rectangle.Bottom));
        }

        public static Quad operator *(OpenTK.Matrix3 m, Quad r)
        {
            return new Quad(
                r.TopLeft * m,
                r.TopRight * m,
                r.BottomLeft * m,
                r.BottomRight * m);
        }

        public OpenTK.Matrix2 BasisTransform
        {
            get
            {
                Vector2 row0 = TopRight - TopLeft;
                Vector2 row1 = BottomLeft - TopLeft;

                if (row0 != Vector2.Zero)
                    row0 /= row0.LengthSquared;

                if (row1 != Vector2.Zero)
                    row1 /= row1.LengthSquared;

                return new OpenTK.Matrix2(
                    row0.X, row0.Y,
                    row1.X, row1.Y);
            }
        }

        public Vector2 Centre => (TopLeft + TopRight + BottomLeft + BottomRight) / 4;
        public Vector2 Size => new Vector2(Width, Height);

        public float Width => Vector2.Distance(TopLeft, TopRight);
        public float Height => Vector2.Distance(TopLeft, BottomLeft);

        public Quad BoundingQuad
        {
            get
            {
                float xMin = Math.Min(TopLeft.X, Math.Min(TopRight.X, Math.Min(BottomLeft.X, BottomRight.X)));
                float xMax = Math.Max(TopLeft.X, Math.Max(TopRight.X, Math.Max(BottomLeft.X, BottomRight.X)));
                float yMin = Math.Min(TopLeft.Y, Math.Min(TopRight.Y, Math.Min(BottomLeft.Y, BottomRight.Y)));
                float yMax = Math.Max(TopLeft.Y, Math.Max(TopRight.Y, Math.Max(BottomLeft.Y, BottomRight.Y)));

                return new Quad(xMin, yMin, xMax - xMin, yMax - yMin);
            }
        }

        public Rectangle BoundingRectangle
        {
            get
            {
                int xMin = (int)Math.Round(Math.Min(TopLeft.X, Math.Min(TopRight.X, Math.Min(BottomLeft.X, BottomRight.X))));
                int xMax = (int)Math.Round(Math.Max(TopLeft.X, Math.Max(TopRight.X, Math.Max(BottomLeft.X, BottomRight.X))));
                int yMin = (int)Math.Round(Math.Min(TopLeft.Y, Math.Min(TopRight.Y, Math.Min(BottomLeft.Y, BottomRight.Y))));
                int yMax = (int)Math.Round(Math.Max(TopLeft.Y, Math.Max(TopRight.Y, Math.Max(BottomLeft.Y, BottomRight.Y))));

                return new Rectangle(xMin, yMin, xMax - xMin, yMax - yMin);
            }
        }

        public Vector2[] Vertices => new Vector2[] { TopLeft, TopRight, BottomRight, BottomLeft };
        public Vector2[] AxisVertices => Vertices;

        public Vector2? Contains(Vector2 pos)
        {
            Vector2? result = new Triangle(BottomRight, BottomLeft, TopRight).Contains(pos);
            if (result.HasValue)
                return Vector2.One - result.Value;

            return new Triangle(TopLeft, TopRight, BottomLeft).Contains(pos);
        }

        public bool Intersects(IConvexPolygon other)
        {
            return (this as IConvexPolygon).Intersects(other);
        }

        public bool Intersects(Rectangle other)
        {
            return (this as IConvexPolygon).Intersects(other);
        }
    }
}
