using System;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public static class Utils
    {
        public static bool IsRoughlyEqual(double a, double b)
        {
            return a * 1.25 > b && a / 1.25 < b;
        }

        public static bool IsRatioEqual(double ratio, double a, double b)
        {
            return a + 15 > ratio * b && a - 15 < ratio * b;
        }

        public static bool IsRatioEqualGreater(double ratio, double a, double b)
        {
            return a + 15 > ratio * b;
        }

        public static bool IsRatioEqualLess(double ratio, double a, double b)
        {
            return a - 15 < ratio * b;
        }

        public static bool IsNullOrNaN(double? nullableDouble)
        {
            return nullableDouble == null || double.IsNaN(nullableDouble.Value);
        }

        /// <summary>
        /// A boolean function that produces non-binary results when the value being checked is between the 100% True and 100% False thresholds.
        /// </summary>
        /// <param name="value">The value being evaluated.</param>
        /// <param name="transitionStart">If the value is at or below this, the result is False.</param>
        /// <param name="transitionInterval">Length of the interval through which the result gradually transitions from False to True.</param>
        /// <returns>Returns a double value from [0, 1] where 0 is 100% False, and 1 is 100% True.</returns>
        public static double TransitionToTrue(double value, double transitionStart, double transitionInterval)
        {
            if (value <= transitionStart)
                return 0;

            if (value >= transitionStart + transitionInterval)
                return 1;

            return (-Math.Cos((value - transitionStart) * Math.PI / transitionInterval) + 1) / 2;
        }

        /// <summary>
        /// A boolean function that produces non-binary results when the value being checked is between the 100% True and 100% False thresholds.
        /// </summary>
        /// <param name="value">The value being evaluated.</param>
        /// <param name="transitionStart">If the value is at or below this, the result is True.</param>
        /// <param name="transitionInterval">Length of the interval through which the result gradually transitions from True to False.</param>
        /// <returns>Returns a double value from [0, 1] where 0 is 100% False, and 1 is 100% True.</returns>
        public static double TransitionToFalse(double value, double transitionStart, double transitionInterval)
        {
            if (value <= transitionStart)
                return 1;

            if (value >= transitionStart + transitionInterval)
                return 0;

            return (Math.Cos((value - transitionStart) * Math.PI / transitionInterval) + 1) / 2;
        }
    }
}