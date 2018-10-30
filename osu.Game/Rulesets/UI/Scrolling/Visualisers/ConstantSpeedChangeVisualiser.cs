// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.UI.Scrolling.Visualisers
{
    public readonly struct ConstantSpeedChangeVisualiser : ISpeedChangeVisualiser
    {
        private readonly double timeRange;
        private readonly float scrollLength;

        public ConstantSpeedChangeVisualiser(double timeRange, float scrollLength)
        {
            this.timeRange = timeRange;
            this.scrollLength = scrollLength;
        }

        public double GetDisplayStartTime(double startTime) => startTime - timeRange;

        public float GetLength(double startTime, double endTime)
        {
            // At the hitobject's end time, the hitobject will be positioned such that its end rests at the origin.
            // This results in a negative-position value, and the absolute of it indicates the length of the hitobject.
            return -PositionAt(endTime, startTime);
        }

        public float PositionAt(double currentTime, double startTime) => (float)((startTime - currentTime) / timeRange * scrollLength);
    }
}
