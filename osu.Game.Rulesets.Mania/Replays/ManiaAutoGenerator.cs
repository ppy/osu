﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Replays;
using osu.Game.Users;

namespace osu.Game.Rulesets.Mania.Replays
{
    internal class ManiaAutoGenerator : AutoGenerator<ManiaHitObject>
    {
        public const double RELEASE_DELAY = 20;

        public new ManiaBeatmap Beatmap => (ManiaBeatmap)base.Beatmap;

        private readonly ManiaAction[] columnActions;

        public ManiaAutoGenerator(ManiaBeatmap beatmap)
            : base(beatmap)
        {
            Replay = new Replay { User = new User { Username = @"Autoplay" } };

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

        protected Replay Replay;

        public override Replay Generate()
        {
            // Todo: Realistically this shouldn't be needed, but the first frame is skipped with the way replays are currently handled
            Replay.Frames.Add(new ManiaReplayFrame(-100000, 0));

            var pointGroups = generateActionPoints().GroupBy(a => a.Time).OrderBy(g => g.First().Time);

            var actions = new List<ManiaAction>();
            foreach (var group in pointGroups)
            {
                foreach (var point in group)
                {
                    if (point is HitPoint)
                        actions.Add(columnActions[point.Column]);
                    if (point is ReleasePoint)
                        actions.Remove(columnActions[point.Column]);
                }

                Replay.Frames.Add(new ManiaReplayFrame(group.First().Time, actions.ToArray()));
            }

            return Replay;
        }

        private IEnumerable<IActionPoint> generateActionPoints()
        {
            foreach (var obj in Beatmap.HitObjects)
            {
                yield return new HitPoint { Time = obj.StartTime, Column = obj.Column };
                yield return new ReleasePoint { Time = ((obj as IHasEndTime)?.EndTime ?? obj.StartTime) + RELEASE_DELAY, Column = obj.Column };
            }
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
