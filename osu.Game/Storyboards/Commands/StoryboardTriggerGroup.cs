// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Storyboards.Commands
{
    public class StoryboardTriggerGroup : StoryboardCommandGroup
    {
        public string TriggerName;
        public double TriggerStartTime;
        public double TriggerEndTime;
        public int GroupNumber;

        public StoryboardTriggerGroup(string triggerName, double startTime, double endTime, int groupNumber)
        {
            TriggerName = triggerName;
            TriggerStartTime = startTime;
            TriggerEndTime = endTime;
            GroupNumber = groupNumber;
        }

        public override string ToString()
            => $"{TriggerName} {TriggerStartTime} -> {TriggerEndTime} ({GroupNumber})";
    }
}
