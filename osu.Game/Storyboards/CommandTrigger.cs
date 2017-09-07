// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Storyboards
{
    public class CommandTrigger : CommandTimelineGroup
    {
        private string triggerName;
        private double startTime;
        private double endTime;
        private int groupNumber;

        public CommandTrigger(string triggerName, double startTime, double endTime, int groupNumber)
        {
            this.triggerName = triggerName;
            this.startTime = startTime;
            this.endTime = endTime;
            this.groupNumber = groupNumber;
        }

        public override string ToString()
            => $"{triggerName} {startTime} -> {endTime} ({groupNumber})";
    }
}
