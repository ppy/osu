// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.MathUtils;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Replays;
using osu.Game.Users;
using osu.Framework.Logging;
namespace osu.Game.Rulesets.Catch.Replays
{
    internal class CatchAutoGenerator2 : AutoGenerator<CatchHitObject>
    {
        public CatchAutoGenerator2(Beatmap<CatchHitObject> beatmap)
            : base(beatmap)
        {
            Replay = new Replay { User = new User { Username = @"osu!salad!" } };
        }

        protected Replay Replay;

        public override Replay Generate()
        {
            float halfCatcherWidth = CatcherArea.GetCatcherSize(Beatmap.BeatmapInfo.BaseDifficulty) / 2;
            double dash_speed = CatcherArea.Catcher.BASE_SPEED;
            // Todo: Realistically this shouldn't be needed, but the first frame is skipped with the way replays are currently handled
            Replay.Frames.Add(new CatchReplayFrame(-100000, 0.5f));
            List<CatchHitObject> objects = new List<CatchHitObject>();
            // List of all catch objects
            foreach (var obj in Beatmap.HitObjects)
            {
                if (obj is Fruit)
                    objects.Add(obj);
                if (obj is BananaShower || obj is JuiceStream)
                    foreach (var nested in obj.NestedHitObjects)
                        objects.Add((CatchHitObject)nested);
            }
            objects.Sort((CatchHitObject h1, CatchHitObject h2) =>
            {
                return h1.StartTime.CompareTo(h2.StartTime) +
                (h1.StartTime.CompareTo(h2.StartTime) == 0 ? h1.HyperDash.CompareTo(h2.HyperDash) : 0); //we need hyper dashes to be last in case of equality.
            });

            //Building the score while skipping droplets/banana that are during an hyperdash from the list
            bool skipping = false;
            List<CatchStepFunction> scores = new List<CatchStepFunction>();
            List<double> times = new List<double>();
            scores.Insert(0, new CatchStepFunction()); // After the last object, there is no more score to be made
            times.Insert(0, 1 + objects[objects.Count - 1].StartTime); //some time after the last object
            for (int i = objects.Count - 1; i >= 0; --i)
            {
                int value;
                CatchHitObject obj = objects[i]; ;
                if (obj is Fruit || obj is Droplet)
                {
                    skipping = obj.HyperDash;
                    value = obj is Fruit ? 300 : 20;
                }
                else if (skipping)
                {
                    objects.RemoveAt(i);
                    continue;
                }
                else
                    value = 1;
                if (obj.StartTime != times[0])
                {
                    scores.Insert(0, new CatchStepFunction(scores[0], (float)(dash_speed * (times[0] - obj.StartTime))));
                    times.Insert(0, obj.StartTime);
                }
                if (obj.HyperDash)
                {
                    float distance = Math.Abs(obj.HyperDashTarget.X - obj.X);
                    scores[0].Set(Math.Max(0, obj.X - halfCatcherWidth), Math.Min(1, obj.X + halfCatcherWidth),
                    scores[0].Max(obj.X - distance, obj.X + distance));
                }
                scores[0].Add(Math.Max(0, obj.X - halfCatcherWidth), Math.Min(1, obj.X + halfCatcherWidth), value);
            }
            Replay.Frames.Add(new CatchReplayFrame(0, 0.5f));
            float lastPosition = 0.5f;
            double lastTime = 0;
            float nexthyperDashDistance = 0;
            float hyperDashDistance = 0;
            for (int i = 0, j = 0; i < objects.Count; ++i)
            {
                if (objects[i].StartTime != times[j])
                {
                    float movementRange = hyperDashDistance == 0 ? (float)(dash_speed * (times[j] - lastTime)) : hyperDashDistance;
                    lastPosition = scores[j].OptimalPath(lastPosition - movementRange, lastPosition + movementRange);
                    lastTime = times[j];
                    Replay.Frames.Add(new CatchReplayFrame(lastTime, lastPosition));
                    ++j;
                    hyperDashDistance = nexthyperDashDistance;
                }
                nexthyperDashDistance = objects[i].HyperDash && lastPosition >= objects[i].X - halfCatcherWidth && lastPosition <= objects[i].X + halfCatcherWidth
                ? Math.Abs(objects[i].HyperDashTarget.X - lastPosition) : 0;
            }
            return Replay;
        }
    }
}
