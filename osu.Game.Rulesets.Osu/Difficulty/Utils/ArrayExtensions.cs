// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Osu.Difficulty.Utils
{
    internal static class ArrayExtensions
    {
        public static T[][][] ToJagged<T>(this T[,,] values)
        {
            int xSize = values.GetLength(0);
            int ySize = values.GetLength(1);
            int zSize = values.GetLength(2);

            var result = new T[xSize][][];

            for (int i = 0; i < xSize; ++i)
            {
                result[i] = new T[ySize][];

                for (int j = 0; j < ySize; ++j)
                {
                    result[i][j] = new T[zSize];

                    for (int k = 0; k < zSize; ++k)
                    {
                        result[i][j][k] = values[i, j, k];
                    }
                }
            }

            return result;
        }
    }
}
