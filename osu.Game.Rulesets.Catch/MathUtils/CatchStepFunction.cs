// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Catch.UI;

namespace osu.Game.Rulesets.Catch.MathUtils
{
    ///<summary>
    /// Step function on [0;<see cref="CatchStepFunction.WIDTH"/>]
    ///</summary>
    public class CatchStepFunction
    {
        public const float WIDTH = CatchPlayfield.WIDTH;

        ///<summary>
        /// Partition of the step function, on [0;<see cref="CatchStepFunction.WIDTH"/>]
        ///</summary>
        private readonly List<float> partition = new List<float>();

        ///<summary>
        /// Values that the step function takes.
        /// value[i] is the value on [partition[i], partition[i+1]]
        ///</summary>
        private readonly List<int> values = new List<int>();

        ///<summary>
        /// Constructs a null function.
        ///</summary>
        public CatchStepFunction()
        {
            partition.Add(0);
            partition.Add(WIDTH);
            values.Add(0);
        }

        ///<summary>
        /// Constructs a step function as the rolling maximum of another, with a set rolling window size.
        ///</summary>
        public CatchStepFunction(CatchStepFunction input, float halfWindowWidth)
        {
            Assert.GreaterOrEqual(halfWindowWidth, 0);

            // windowsLeft is the index of the first input partition that is strictly greater than the left of the window
            // windowsRight is the index of the first input partition that is strictly greater than the right of the window
            int windowLeft = 0, windowRight;
            Queue<int> window = new Queue<int>();

            // Extend the input function left and right, to simplify things
            input.partition.Add(WIDTH + halfWindowWidth);
            input.values.Add(0);
            input.partition.Insert(0, -halfWindowWidth);
            input.values.Insert(0, 0);

            // Initialising the window.
            for (windowRight = windowLeft; input.partition[windowRight] <= halfWindowWidth; ++windowRight)
                window.Enqueue(input.values[windowRight]);
            var windowMax = window.Max();
            ++windowLeft;

            // At each iteration we slide the windows one step to the right,
            // adding a new value and partition each time, until the end.
            partition.Add(0);

            while (true)
            {
                values.Add(windowMax);
                // This distance is used to know if it is the left side or the right side of the windows
                // that will meet with the next partition first. (or both at the same time if the distance is 0)
                float distance = input.partition[windowRight] - input.partition[windowLeft] - 2 * halfWindowWidth;

                if (distance <= 0)
                {
                    //if we reach the end, stop. The last partition (1) is added after the loop.
                    if (windowRight == input.partition.Count - 1)
                        break;

                    windowMax = Math.Max(windowMax, input.values[windowRight]);
                    window.Enqueue(input.values[windowRight]);
                    partition.Add(input.partition[windowRight] - halfWindowWidth);
                    ++windowRight;
                }

                if (distance >= 0)
                {
                    if (window.Dequeue() == windowMax)
                        windowMax = window.Max();
                    partition.Add(input.partition[windowLeft] + halfWindowWidth);
                    ++windowLeft;
                }

                //if we added the same partition twice (moving two steps in a iteration)
                if (distance == 0)
                    partition.RemoveAt(partition.Count - 1);
            }

            partition.Add(WIDTH);

            // Revert the extension
            input.partition.RemoveAt(0);
            input.partition.RemoveAt(input.partition.Count - 1);
            input.values.RemoveAt(0);
            input.values.RemoveAt(input.values.Count - 1);

            cleanup();
        }

        ///<summary>
        /// Removes redundant Partition.
        ///</summary>
        private void cleanup()
        {
            for (int i = values.Count - 1; i > 1; --i)
            {
                if (values[i] == values[i - 1])
                {
                    values.RemoveAt(i);
                    partition.RemoveAt(i);
                }
            }

            for (int i = partition.Count - 1; i > 1; --i)
            {
                if (partition[i] == partition[i - 1])
                {
                    values.RemoveAt(i - 1);
                    partition.RemoveAt(i);
                }
            }
        }

        ///<summary>
        /// Adds <param name="value"></param> time the indicator function of
        /// [<param name="from"></param>, <param name="to"></param>] to the step function.
        ///</summary>
        public void Add(float from, float to, int value)
        {
            Assert.GreaterOrEqual(from, 0);
            Assert.GreaterOrEqual(to, from);
            Assert.GreaterOrEqual(WIDTH, to);

            int indexStart, indexEnd;

            for (indexStart = 0; partition[indexStart] <= from; ++indexStart)
            {
            }

            partition.Insert(indexStart, from);
            values.Insert(indexStart, values[indexStart - 1]);

            for (indexEnd = indexStart; partition[indexEnd] < to; ++indexEnd)
            {
            }

            partition.Insert(indexEnd, to);
            values.Insert(indexEnd - 1, values[indexEnd - 1]);
            for (int i = indexStart; i < indexEnd; ++i)
                values[i] += value;
            cleanup();
        }

        public void Set(float from, float to, int value)
        {
            Assert.GreaterOrEqual(from, 0);
            Assert.GreaterOrEqual(to, from);
            Assert.GreaterOrEqual(WIDTH, to);

            int indexStart, indexEnd;

            for (indexStart = 0; partition[indexStart] <= from; ++indexStart)
            {
            }

            partition.Insert(indexStart, from);
            values.Insert(indexStart, values[indexStart - 1]);

            for (indexEnd = indexStart; partition[indexEnd] < to; ++indexEnd)
            {
            }

            partition.Insert(indexEnd, to);
            values.Insert(indexEnd - 1, values[indexEnd - 1]);
            for (int i = indexStart; i < indexEnd; ++i)
                values[i] = value;
            cleanup();
        }

        ///<summary>
        /// Maximal value on [<param name="from"></param>, <param name="to"></param>]
        ///</summary>
        public int Max(float from, float to)
        {
            int max = 0;

            for (int i = 0; i < values.Count; ++i)
            {
                if (values[i] > max && partition[i] < to && partition[i + 1] > from)
                    max = values[i];
            }

            return max;
        }

        ///<summary>
        /// Returns a point of [<paramref name="from"></paramref>, <paramref name="to"></paramref>] that reach the
        /// maximal value on [<paramref name="from"></paramref>, <paramref name="to"></paramref>].
        /// Returns <paramref name="target"></paramref> if it works,
        /// we return the point furthest away from a suboptimal point otherwise,
        /// as it will often be the easiest optimal path, from a gameplay perspective.
        ///</summary>
        public float OptimalPath(float from, float to, float target)
        {
            Assert.GreaterOrEqual(to, target);
            Assert.GreaterOrEqual(target, from);

            int max = Max(from, to);
            float ret = -1, value = -1;

            for (int i = 0; i < values.Count; ++i)
            {
                if (values[i] == max && partition[i] <= to && partition[i + 1] >= from)
                {
                    if (target >= partition[i] && target <= partition[i + 1])
                        return target;

                    float newValue = partition[i + 1] - partition[i];

                    if (newValue > value)
                    {
                        value = newValue;
                        ret = Math.Clamp((partition[i + 1] + partition[i]) / 2, from, to);
                    }
                }
            }

            return ret;
        }
    }
}
