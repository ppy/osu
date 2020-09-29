using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    public class Transform
    {
        public delegate double Func(double d);
        public Func Offset { get; set; } = d => 0;
        public Func Scale { get; set; } = d => 1;

        public double Apply(double d, double x)
        {
            return (x - Offset(d))/Scale(d);
        }

        public double Inverse(double d, double Fx)
        {
            return Fx * Scale(d) + Offset(d);
        }

        public static double Apply(Transform transform, double d, double x)
        {
            return transform?.Apply(d, x) ?? x;
        }

        public static double Inverse(Transform transform, double d, double x)
        {
            return transform?.Inverse(d, x) ?? x;
        }

    }
}
