//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Framework.Extensions.MatrixExtensions
{
    public static class MatrixExtensions
    {
        public static Matrix3 TranslateTo(this Matrix3 m, Vector2 v)
        {
            m.Row2 += m.Row0 * v.X + m.Row1 * v.Y;

            return m;
        }

        public static Matrix3 RotateTo(this Matrix3 m, float angle)
        {
            // Convert to radians
            angle = angle / (180 / MathHelper.Pi);
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);

            Vector3 temp = m.Row0 * cos + m.Row1 * sin;
            m.Row1 = m.Row1 * cos - m.Row0 * sin;
            m.Row0 = temp;

            return m;
        }

        public static Matrix3 ScaleTo(this Matrix3 m, Vector2 v)
        {
            m.Row0 *= v.X;
            m.Row1 *= v.Y;

            return m;
        }
    }
}
