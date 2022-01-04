// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Mania.Replays
{
    internal class ManiaAutoGenerator : AutoGenerator<ManiaReplayFrame>
    {
        public const double RELEASE_DELAY = 20;

        public new ManiaBeatmap Beatmap => (ManiaBeatmap)base.Beatmap;

        private readonly ManiaAction[] columnActions;

        public ManiaAutoGenerator(ManiaBeatmap beatmap)
            : base(beatmap)
        {
            columnActions = new ManiaAction[Beatmap.TotalColumns];

            var normalAction = ManiaAction.Key1;
            var specialAction = ManiaAction.Special1;
            int totalCounter = 0;

            foreach (var stage in Beatmap.Stages)
            {
                for (int i = 0; i < stage.Columns; i++)
                {
                    if (stage.IsSpecialColumn(i))
                        columnActions[totalCounter] = specialAction++;
                    else
                        columnActions[totalCounter] = normalAction++;
                    totalCounter++;
                }
            }
        }

        protected override void GenerateFrames()
        {
            if (Beatmap.HitObjects.Count == 0)
                return;

            var pointGroups = generateActionPoints().GroupBy(a => a.Time).OrderBy(g => g.First().Time);

            var actions = new List<ManiaAction>();

            foreach (var group in pointGroups)
            {
                foreach (var point in group)
                {
                    switch (point)
                    {
                        case HitPoint _:
                            actions.Add(columnActions[point.Column]);
                            break;

                        case ReleasePoint _:
                            actions.Remove(columnActions[point.Column]);
                            break;
                    }
                }

                Frames.Add(new ManiaReplayFrame(group.First().Time, actions.ToArray()));
            }
        }

        private IEnumerable<IActionPoint> generateActionPoints()
        {
            for (int i = 0; i < Beatmap.HitObjects.Count; i++)
            {
                var currentObject = Beatmap.HitObjects[i];
                var nextObjectInColumn = GetNextObject(i); // Get the next object that requires pressing the same button
                double releaseTime = calculateReleaseTime(currentObject, nextObjectInColumn);

                yield return new HitPoint { Time = currentObject.StartTime, Column = currentObject.Column };

                yield return new ReleasePoint { Time = releaseTime, Column = currentObject.Column };
            }
        }

        private double calculateReleaseTime(HitObject currentObject, HitObject nextObject)
        {
            double endTime = currentObject.GetEndTime();

            if (currentObject is HoldNote)
                // hold note releases must be timed exactly.
                return endTime;

            bool canDelayKeyUpFully = nextObject == null ||
                                      nextObject.StartTime > endTime + RELEASE_DELAY;

            return endTime + (canDelayKeyUpFully ? RELEASE_DELAY : (nextObject.StartTime - endTime) * 0.9);
        }

        protected override HitObject GetNextObject(int currentIndex)
        {
            int desiredColumn = Beatmap.HitObjects[currentIndex].Column;

            for (int i = currentIndex + 1; i < Beatmap.HitObjects.Count; i++)
            {
                if (Beatmap.HitObjects[i].Column == desiredColumn)
                    return Beatmap.HitObjects[i];
            }

            return null;
        }

        private interface IActionPoint
        {
            double Time { get; set; }
            int Column { get; set; }
        }

        private struct HitPoint : IActionPoint
        {
            public double Time { get; set; }
            public int Column { get; set; }
        }

        private struct ReleasePoint : IActionPoint
        {
            public double Time { get; set; }
            public int Column { get; set; }
        }
    }
}
