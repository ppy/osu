//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Extensions.MatrixExtensions;

namespace osu.Framework.Graphics
{
    public class DrawInfo : IEquatable<DrawInfo>
    {
        public Matrix3 Matrix = Matrix3.Identity;
        public Matrix3 MatrixInverse => matrixInverse ?? (matrixInverse = Matrix.Inverted()).Value;
        public Color4 Colour = Color4.White;

        private Matrix3? matrixInverse;

        public DrawInfo()
        {
        }

        /// <summary>
        /// Applies a transformation to the current DrawInfo.
        /// </summary>
        /// <param name="target">The DrawInfo instance to be filled with the result.</param>
        /// <param name="translation">The amount by which to translate the current position.</param>
        /// <param name="scale">The amount by which to scale.</param>
        /// <param name="rotation">The amount by which to rotate.</param>
        /// <param name="origin">The center of rotation and scale.</param>
        /// <param name="colour">An optional color to be applied multiplicatively.</param>
        /// <param name="viewport">An optional new viewport size.</param>
        public void ApplyTransform(DrawInfo target, Vector2 translation, Vector2 scale, float rotation, Vector2 origin, Color4? colour = null)
        {
            Matrix3 m = Matrix;

            if (translation != Vector2.Zero)
                m = m.TranslateTo(translation);
            if (rotation != 0)
                m = m.RotateTo(rotation);
            if (scale != Vector2.One)
                m = m.ScaleTo(scale);
            if (origin != Vector2.Zero)
                m = m.TranslateTo(-origin);

            target.Matrix = m;

            target.Colour = Colour;

            if (colour != null)
            {
                target.Colour.R *= colour.Value.R;
                target.Colour.G *= colour.Value.G;
                target.Colour.B *= colour.Value.B;
                target.Colour.A *= colour.Value.A;
            }
        }

        /// <summary>
        /// Copies the current DrawInfo into target.
        /// </summary>
        /// <param name="target">The DrawInfo to be filled with the copy.</param>
        public void Copy(DrawInfo target)
        {
            target.Matrix = Matrix;
            target.Colour = Colour;
        }

        public bool Equals(DrawInfo other)
        {
            return Matrix.Equals(other.Matrix) && Colour.Equals(other.Colour);
        }
    }
}
