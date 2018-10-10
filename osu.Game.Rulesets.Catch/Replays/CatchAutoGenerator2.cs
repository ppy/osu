// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.MathUtils;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Replays;
using osu.Game.Users;
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
            const double dash_speed = CatcherArea.Catcher.BASE_SPEED;
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

            // Removing droplets/banana that are during an hyperdash from the list
            bool skipping = false;
            for (int i = 0; i < objects.Count; ++i)
                if (skipping && (objects[i] is Banana || objects[i] is TinyDroplet))
                    objects.RemoveAt(i--);
                else
                    skipping = objects[i].HyperDash;

            //Building the score
            List<CatchStepFunction> scores = new List<CatchStepFunction>();
            List<double> times = new List<double>();
            scores.Insert(0, new CatchStepFunction()); // After the last object, there is no more score to be made
            times.Insert(0, 1 + objects[objects.Count - 1].StartTime); //some time after the last object
            for (int i = objects.Count - 1; i >= 0; --i)
            {
                CatchHitObject obj = objects[i];
                int value = obj is Banana || obj is TinyDroplet ? 1 : obj is Fruit ? 300 : 20;
                if (obj.StartTime != times[0])
                {
                    scores.Insert(0, new CatchStepFunction(scores[0], (float)(dash_speed * (times[0] - obj.StartTime))));
                    times.Insert(0, obj.StartTime);
                }
                if (obj.HyperDash)
                {
                    float distance = Math.Abs(obj.HyperDashTarget.X - obj.X);
                    scores[0].Set(Math.Max(0, obj.X - halfCatcherWidth), Math.Min(1, obj.X + halfCatcherWidth),
                    scores[1].Max(obj.X - distance, obj.X + distance));
                }
                scores[0].Add(Math.Max(0, obj.X - halfCatcherWidth), Math.Min(1, obj.X + halfCatcherWidth), value);
            }
            float lastPosition = 0.5f;
            double lastTime = -10000;
            void moveToNext(float target, double time)
            {
                const double movementSpeed = dash_speed / 2;
                float positionChange = Math.Abs(lastPosition - target);
                double timeAvailable = time - lastTime;
                //So we can either make it there without a dash or not.
                double speedRequired = positionChange / timeAvailable;
                bool dashRequired = speedRequired > movementSpeed && time != 0;
                if (dashRequired)
                {
                    //we do a movement in two parts - the dash part then the normal part...
                    double timeAtDashSpeed = (positionChange - movementSpeed * timeAvailable) / (dash_speed - movementSpeed);
                    if (timeAtDashSpeed <= timeAvailable)
                    {
                        float midPosition = lastPosition + Math.Sign(target - lastPosition) * (float)(timeAtDashSpeed * dash_speed);
                        //dash movement
                        Replay.Frames.Add(new CatchReplayFrame(lastTime + timeAtDashSpeed, midPosition, true));
                        Replay.Frames.Add(new CatchReplayFrame(time, target));
                    }
                    else
                        Replay.Frames.Add(new CatchReplayFrame(time, target, true));
                }
                else
                {
                    double timeBefore = positionChange / movementSpeed;

                    Replay.Frames.Add(new CatchReplayFrame(lastTime + timeBefore, target));
                    Replay.Frames.Add(new CatchReplayFrame(time, target));
                }
                lastTime = time;
                lastPosition = target;
            }
            moveToNext(0.5f, 0);
            float hyperDashDistance = 0;
            for (int i = 0, j = 0; i < objects.Count; ++i)
            {
                if (objects[i].StartTime != lastTime)
                {
                    float movementRange = hyperDashDistance == 0 ? (float)(dash_speed * (times[j] - lastTime)) : hyperDashDistance;
                    float target = scores[j].OptimalPath(lastPosition - movementRange, lastPosition + movementRange, lastPosition);
                    moveToNext(target, times[j++]);
                }
                hyperDashDistance = objects[i].HyperDash && lastPosition >= objects[i].X - halfCatcherWidth && lastPosition <= objects[i].X + halfCatcherWidth
                ? Math.Abs(objects[i].HyperDashTarget.X - lastPosition) : 0;
            }
            return Replay;
        }
    }
}
