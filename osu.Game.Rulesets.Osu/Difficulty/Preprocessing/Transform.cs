// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    public class Transform
    {
        public delegate double Func(double d);
        public Func XOffset { get; set; } = d => 0;
        public Func YOffset { get; set; } = d => 0;
        public Func Scale { get; set; } = d => 1;

        public (double, double) Apply(double d, double x, double y)
        {
            double scale = 1 / Scale(d);
            return (scale * (x - XOffset(d)), scale * (y - YOffset(d)));
        }

        public (double, double) Inverse(double d, double Fx, double Fy)
        {
            double scale = Scale(d);
            return (Fx * scale + XOffset(d), Fy * scale + YOffset(d));
        }

        public static (double, double) Apply(Transform transform, double d, double x, double y)
        {
            return transform?.Apply(d, x, y) ?? (x, y);
        }

        public static (double, double) Inverse(Transform transform, double d, double x, double y)
        {
            return transform?.Inverse(d, x, y) ?? (x, y);
        }

    }
}
