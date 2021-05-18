using System;

namespace Mvis.Plugin.RulesetPanel.Extensions
{
    public static class ArrayExtensions
    {
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
