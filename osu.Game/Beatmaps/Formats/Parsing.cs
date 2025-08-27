// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osuTK;
using System.Text.RegularExpressions;

namespace osu.Game.Beatmaps.Formats
{
    /// <summary>
    /// Helper methods to parse from string to number and perform very basic validation.
    /// </summary>
    public static class Parsing
    {
        public const int MAX_COORDINATE_VALUE = 131072;

        public const double MAX_PARSE_VALUE = int.MaxValue;

        public const string VECTOR_PATTERN = @"\((.+)\; (.+)\)";

        public static float ParseFloat(string input, float parseLimit = (float)MAX_PARSE_VALUE, bool allowNaN = false)
        {
            float output = float.Parse(input, CultureInfo.InvariantCulture);

            if (output < -parseLimit) throw new OverflowException("Value is too low");
            if (output > parseLimit) throw new OverflowException("Value is too high");

            if (!allowNaN && float.IsNaN(output)) throw new FormatException("Not a number");

            return output;
        }

        public static double ParseDouble(string input, double parseLimit = MAX_PARSE_VALUE, bool allowNaN = false)
        {
            double output = double.Parse(input, CultureInfo.InvariantCulture);

            if (output < -parseLimit) throw new OverflowException("Value is too low");
            if (output > parseLimit) throw new OverflowException("Value is too high");

            if (!allowNaN && double.IsNaN(output)) throw new FormatException("Not a number");

            return output;
        }

        public static int ParseInt(string input, int parseLimit = (int)MAX_PARSE_VALUE)
        {
            int output = int.Parse(input, CultureInfo.InvariantCulture);

            if (output < -parseLimit) throw new OverflowException("Value is too low");
            if (output > parseLimit) throw new OverflowException("Value is too high");

            return output;
        }

        public static Vector2 ParseVector2(string input)
        {
            // I hope it's correct to do it that way, since otherwise idk how else can I set boundaries. Technically editor handles it on it's own, but idk
            Vector2 parseLimit = new Vector2(MAX_COORDINATE_VALUE, MAX_COORDINATE_VALUE);

            Match vectorCoords = Regex.Match(input, VECTOR_PATTERN);
            Vector2 output = new Vector2(float.Parse(vectorCoords.Groups[1].Value, CultureInfo.InvariantCulture), float.Parse(vectorCoords.Groups[2].Value, CultureInfo.InvariantCulture));

            if (output.X < -parseLimit.X) throw new OverflowException("Value X is too low");
            if (output.Y < -parseLimit.Y) throw new OverflowException("Value Y is too low");
            if (output.X > parseLimit.X) throw new OverflowException("Value X is too high");
            if (output.Y > parseLimit.Y) throw new OverflowException("Value Y is too high");

            return output;
        }
    }
}
