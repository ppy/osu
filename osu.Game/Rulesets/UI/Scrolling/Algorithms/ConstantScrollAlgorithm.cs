// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.UI.Scrolling.Algorithms
{
    public class ConstantScrollAlgorithm : IScrollAlgorithm
    {
        public double GetDisplayStartTime(double time, double timeRange) => time - timeRange;

        public float GetLength(double startTime, double endTime, double timeRange, float scrollLength) => -PositionAt(startTime, endTime, timeRange, scrollLength);

        public float PositionAt(double time, double currentTime, double timeRange, float scrollLength)
            => (float)((time - currentTime) / timeRange * scrollLength);

        public double TimeAt(float position, double currentTime, double timeRange, float scrollLength)
            => position * timeRange / scrollLength + currentTime;

        public void Reset()
        {
        }
    }
}
