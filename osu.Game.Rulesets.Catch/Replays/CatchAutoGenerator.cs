// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Replays;
using osu.Game.Users;

namespace osu.Game.Rulesets.Catch.Replays
{
    internal class CatchAutoGenerator : AutoGenerator<CatchHitObject>
    {
        public const double RELEASE_DELAY = 20;

        public CatchAutoGenerator(Beatmap<CatchHitObject> beatmap)
            : base(beatmap)
        {
            Replay = new Replay { User = new User { Username = @"Autoplay" } };
        }

        protected Replay Replay;

        public override Replay Generate()
        {
            // todo: add support for HT DT
            const double dash_speed = CatcherArea.Catcher.BASE_SPEED;
            const double movement_speed = dash_speed / 2;
            float lastPosition = 0.5f;
            double lastTime = 0;
            bool hyperTarget = false;

            // Todo: Realistically this shouldn't be needed, but the first frame is skipped with the way replays are currently handled
            Replay.Frames.Add(new CatchReplayFrame(-100000, lastPosition));

            void moveToNext(CatchHitObject h)
            {
                float positionChange = Math.Abs(lastPosition - h.X);
                double timeAvailable = h.StartTime - lastTime;

                //So we can either make it there without a dash or not.
                double speedRequired = positionChange / timeAvailable;

                bool dashRequired = speedRequired > movement_speed && h.StartTime != 0;

                float catcherWidthHalf = CatcherArea.GetCatcherSize(Beatmap.BeatmapInfo.BaseDifficulty) / 2;

                if (lastPosition - catcherWidthHalf <= h.X && lastPosition + catcherWidthHalf >= h.X)
                {
                    //we are already in the correct range.
                    lastTime = h.StartTime;
                    Replay.Frames.Add(new CatchReplayFrame(h.StartTime, lastPosition));
                    return;
                }
                else if (h is Banana)
                    // auto bananas unrealistically warp to catch 100% combo.
                    Replay.Frames.Add(new CatchReplayFrame(h.StartTime, h.X));
                else if (dashRequired)
                {
                    //we do a movement in two parts - the dash part then the normal part...
                    double timeAtDashSpeed = (positionChange - movement_speed * timeAvailable) / (dash_speed - movement_speed);
                    if (timeAtDashSpeed <= timeAvailable)
                    {
                        float midPosition = lastPosition + Math.Sign(h.X - lastPosition) * (float)(timeAtDashSpeed * dash_speed);
                        //dash movement
                        Replay.Frames.Add(new CatchReplayFrame(lastTime + timeAtDashSpeed, midPosition, true));
                        Replay.Frames.Add(new CatchReplayFrame(h.StartTime, h.X));
                    }
                    else
                        Replay.Frames.Add(new CatchReplayFrame(h.StartTime, h.X, true));
                }
                else
                {
                    double timeBefore = positionChange / movement_speed;

                    Replay.Frames.Add(new CatchReplayFrame(lastTime + timeBefore, h.X));
                    Replay.Frames.Add(new CatchReplayFrame(h.StartTime, h.X));
                }

                lastTime = h.StartTime;
                lastPosition = h.X;
            }

            foreach (var obj in Beatmap.HitObjects)
            {
                switch (obj)
                {
                    case Fruit _:
                        moveToNext(obj);
                        hyperTarget = obj.HyperDash;
                        break;
                }

                foreach (var nestedObj in obj.NestedHitObjects.Cast<CatchHitObject>())
                {
                    switch (nestedObj)
                    {
                        case Banana _:
                        case TinyDroplet _:
                            if (!hyperTarget)
                                moveToNext(nestedObj);
                            break;
                        case Droplet _:
                        case Fruit _:
                            moveToNext(nestedObj);
                            hyperTarget = nestedObj.HyperDash;
                            break;
                    }
                }
            }

            return Replay;
        }
    }
}
