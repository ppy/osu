using System;
using osuTK;

namespace Mvis.Plugin.RulesetPanel.Extensions
{
    public static class MathExtensions
    {
        public static float Map(float value, float minValue, float maxValue, float minEndValue, float maxEndValue)
        {
            return (value - minValue) / (maxValue - minValue) * (maxEndValue - minEndValue) + minEndValue;
        }

        public static double Map(double value, double minValue, double maxValue, double minEndValue, double maxEndValue)
        {
            return (value - minValue) / (maxValue - minValue) * (maxEndValue - minEndValue) + minEndValue;
        }

        public static Vector2 Map(float value, float minValue, float maxValue, Vector2 minEndValue, Vector2 maxEndValue)
        {
            return new Vector2(Map(value, minValue, maxValue, minEndValue.X, maxEndValue.X), Map(value, minValue, maxValue, minEndValue.Y, maxEndValue.Y));
        }

        public static void Smooth(this float[] src, int severity = 1)
        {
            for (int i = 0; i < src.Length; i++)
            {
                var start = Math.Max(i - severity, 0);
                var end = Math.Min(i + severity, src.Length);

                float sum = 0;

                for (int j = start; j < end; j++)
                    sum += src[j];

                src[i] = sum / (end - start);
            }
        }
    }
}
