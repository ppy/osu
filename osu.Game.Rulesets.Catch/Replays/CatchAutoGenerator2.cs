// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Catch.MathUtils;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Catch.Replays
{
    internal class CatchAutoGenerator2 : AutoGenerator
    {
        protected new Beatmap<CatchHitObject> Beatmap => (Beatmap<CatchHitObject>)base.Beatmap;

        public CatchAutoGenerator2(Beatmap<CatchHitObject> beatmap)
            : base(beatmap)
        {
            Replay = new Replay();
        }

        protected Replay Replay;

        public override Replay Generate()
        {
            float halfCatcherWidth = Catcher.CalculateCatchWidth(Beatmap.BeatmapInfo.BaseDifficulty) / 2;
            const double dash_speed = Catcher.BASE_SPEED;
            // Todo: Realistically this shouldn't be needed, but the first frame is skipped with the way replays are currently handled
            Replay.Frames.Add(new CatchReplayFrame(-100000, 0.5f));
            List<PalpableCatchHitObject> objects = new List<PalpableCatchHitObject>();
            // List of all catch objects
            foreach (var obj in Beatmap.HitObjects)
            {
                if (obj is Fruit fruit)
                    objects.Add(fruit);
                if (obj is BananaShower || obj is JuiceStream)
                    foreach (var nested in obj.NestedHitObjects.OfType<PalpableCatchHitObject>())
                        objects.Add(nested);
            }

            objects.Sort((h1, h2) => h1.StartTime.CompareTo(h2.StartTime) +
                                     (h1.StartTime.CompareTo(h2.StartTime) == 0 ? h1.HyperDash.CompareTo(h2.HyperDash) : 0));

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
                var obj = objects[i];
                int value = obj is Banana || obj is TinyDroplet ? 1 : obj is Fruit ? 300 : 20;
                if (obj.StartTime != times[0])
                {
                    scores.Insert(0, new CatchStepFunction(scores[0], (float)(dash_speed * (times[0] - obj.StartTime))));
                    times.Insert(0, obj.StartTime);
                }

                if (obj.HyperDash)
                {
                    float distance = Math.Abs(obj.HyperDashTarget.EffectiveX - obj.EffectiveX);
                    scores[0].Set(Math.Max(0, obj.EffectiveX - halfCatcherWidth), Math.Min(1, obj.EffectiveX + halfCatcherWidth),
                        scores[1].Max(obj.EffectiveX - distance, obj.EffectiveX + distance));
                }

                scores[0].Add(Math.Max(0, obj.EffectiveX - halfCatcherWidth), Math.Min(1, obj.EffectiveX + halfCatcherWidth), value);
            }

            float lastPosition = 0.5f;
            double lastTime = -10000;

            void moveToNext(float target, double time)
            {
                const double movement_speed = dash_speed / 2;
                float positionChange = Math.Abs(lastPosition - target);
                double timeAvailable = time - lastTime;
                //So we can either make it there without a dash or not.
                double speedRequired = positionChange / timeAvailable;
                bool dashRequired = speedRequired > movement_speed && time != 0;
                if (dashRequired)
                {
                    //we do a movement in two parts - the dash part then the normal part...
                    double timeAtDashSpeed = (positionChange - movement_speed * timeAvailable) / (dash_speed - movement_speed);
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
                    double timeBefore = positionChange / movement_speed;

                    Replay.Frames.Add(new CatchReplayFrame(lastTime + timeBefore, target));
                    Replay.Frames.Add(new CatchReplayFrame(time, target));
                }

                lastTime = time;
                lastPosition = target;
            }

            moveToNext(0.5f, -1000);
            float hyperDashDistance = 0;
            int j = 0;
            foreach (var obj in objects)
            {
                if (obj.StartTime != lastTime)
                {
                    float movementRange = hyperDashDistance == 0 ? (float)(dash_speed * (times[j] - lastTime)) : hyperDashDistance;
                    float target = scores[j].OptimalPath(lastPosition - movementRange, lastPosition + movementRange, lastPosition);
                    moveToNext(target, times[j++]);
                }

                hyperDashDistance = obj.HyperDash && lastPosition >= obj.EffectiveX - halfCatcherWidth && lastPosition <= obj.EffectiveX + halfCatcherWidth
                    ? Math.Abs(obj.HyperDashTarget.EffectiveX - lastPosition)
                    : 0;
            }

            return Replay;
        }
    }
}
