// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Replays;
using osu.Game.Users;

namespace osu.Game.Rulesets.Mania.Replays
{
    internal class ManiaAutoGenerator : AutoGenerator<ManiaHitObject>
    {
        private const double release_delay = 20;

        private readonly int availableColumns;

        public ManiaAutoGenerator(Beatmap<ManiaHitObject> beatmap, int availableColumns)
            : base(beatmap)
        {
            this.availableColumns = availableColumns;

            Replay = new Replay { User = new User { Username = @"Autoplay" } };
        }

        protected Replay Replay;

        public override Replay Generate()
        {
            // Todo: Realistically this shouldn't be needed, but the first frame is skipped with the way replays are currently handled
            Replay.Frames.Add(new ReplayFrame(-100000, null, null, ReplayButtonState.None));

            double[] holdEndTimes = new double[availableColumns];
            for (int i = 0; i < availableColumns; i++)
                holdEndTimes[i] = double.NegativeInfinity;

            // Notes are handled row-by-row
            foreach (var objGroup in Beatmap.HitObjects.GroupBy(h => h.StartTime))
            {
                double groupTime = objGroup.Key;

                int activeColumns = 0;

                // Get the previously held-down active columns
                for (int i = 0; i < availableColumns; i++)
                {
                    if (holdEndTimes[i] > groupTime)
                        activeColumns |= 1 << i;
                }

                // Add on the group columns, keeping track of the held notes for the next rows
                foreach (var obj in objGroup)
                {
                    var holdNote = obj as HoldNote;
                    if (holdNote != null)
                        holdEndTimes[obj.Column] = Math.Max(holdEndTimes[obj.Column], holdNote.EndTime);

                    activeColumns |= 1 << obj.Column;
                }

                Replay.Frames.Add(new ReplayFrame(groupTime, activeColumns, null, ReplayButtonState.None));

                // Add the release frames. We can't do this with the loop above because we need activeColumns to be fully populated
                foreach (var obj in objGroup.GroupBy(h => (h as IHasEndTime)?.EndTime ?? h.StartTime + release_delay).OrderBy(h => h.Key))
                {
                    var groupEndTime = obj.Key;

                    int activeColumnsAtEnd = 0;
                    for (int i = 0; i < availableColumns; i++)
                    {
                        if (holdEndTimes[i] > groupEndTime)
                            activeColumnsAtEnd |= 1 << i;
                    }

                    Replay.Frames.Add(new ReplayFrame(groupEndTime, activeColumnsAtEnd, 0, ReplayButtonState.None));
                }
            }

            Replay.Frames = Replay.Frames
                                  // Pick the maximum activeColumns for all frames at the same time
                                  .GroupBy(f => f.Time)
                                  .Select(g => new ReplayFrame(g.First().Time, maxMouseX(g), 0, ReplayButtonState.None))
                                  // The addition of release frames above maybe result in unordered frames, but we need them ordered
                                  .OrderBy(f => f.Time)
                                  .ToList();

            return Replay;
        }

        /// <summary>
        /// Finds the maximum <see cref="ReplayFrame.MouseX"/> by count of bits from a grouping of <see cref="ReplayFrame"/>s.
        /// </summary>
        /// <param name="group">The <see cref="ReplayFrame"/> grouping to search.</param>
        /// <returns>The maximum <see cref="ReplayFrame.MouseX"/> by count of bits.</returns>
        private float maxMouseX(IGrouping<double, ReplayFrame> group)
        {
            int currentCount = -1;
            int currentMax = 0;

            foreach (var val in group)
            {
                int newCount = countBits((int)(val.MouseX ?? 0));
                if (newCount > currentCount)
                {
                    currentCount = newCount;
                    currentMax = (int)(val.MouseX ?? 0);
                }
            }

            return currentMax;
        }

        /// <summary>
        /// Counts the number of bits set in a value.
        /// </summary>
        /// <param name="value">The value to count.</param>
        /// <returns>The number of set bits.</returns>
        private int countBits(int value)
        {
            int count = 0;
            while (value > 0)
            {
                if ((value & 1) > 0)
                    count++;
                value >>= 1;
            }

            return count;
        }
    }
}
