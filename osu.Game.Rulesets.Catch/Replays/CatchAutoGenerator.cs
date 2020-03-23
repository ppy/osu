// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Catch.Replays
{
    internal class CatchAutoGenerator : AutoGenerator
    {
        public const double RELEASE_DELAY = 20;

        public new CatchBeatmap Beatmap => (CatchBeatmap)base.Beatmap;

        public CatchAutoGenerator(IBeatmap beatmap)
            : base(beatmap)
        {
            Replay = new Replay();
        }

        protected Replay Replay;

        private CatchReplayFrame currentFrame;

        public override Replay Generate()
        {
            // todo: add support for HT DT
            const double dash_speed = Catcher.BASE_SPEED;
            const double movement_speed = dash_speed / 2;
            float position = 0.5f;
            double lastTime = 0;
            bool hyperTarget = false;

            void moveToNext(CatchHitObject h, ref float lastPosition)
            {
                float positionChange = Math.Abs(lastPosition - h.X);
                double timeAvailable = h.StartTime - lastTime;

                // So we can either make it there without a dash or not.
                // If positionChange is 0, we don't need to move, so speedRequired should also be 0 (could be NaN if timeAvailable is 0 too)
                // The case where positionChange > 0 and timeAvailable == 0 results in PositiveInfinity which provides expected beheaviour.
                double speedRequired = positionChange == 0 ? 0 : positionChange / timeAvailable;

                bool dashRequired = speedRequired > movement_speed;

                float catcherWidthHalf = CatcherArea.GetCatcherSize(Beatmap.BeatmapInfo.BaseDifficulty) / 2;

                if (lastPosition - catcherWidthHalf <= h.X && lastPosition + catcherWidthHalf >= h.X)
                {
                    //we are already in the correct range.
                    lastTime = h.StartTime;
                    addFrame(h.StartTime, lastPosition);
                    return;
                }
                else if (h is Banana)
                {
                    // auto bananas unrealistically warp to catch 100% combo.
                    Replay.Frames.Add(new CatchReplayFrame(h.StartTime, h.X));
                }
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
                        moveToNext(obj, ref position);
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
                                moveToNext(nestedObj, ref position);
                            break;

                        case Droplet _:
                        case Fruit _:
                            moveToNext(nestedObj, ref position);
                            hyperTarget = nestedObj.HyperDash;
                            break;
                    }
                }
            }

            return Replay;
        }

        private void addFrame(double time, float? position = null, bool dashing = false)
        {
            // todo: can be removed once FramedReplayInputHandler correctly handles rewinding before first frame.
            if (Replay.Frames.Count == 0)
                Replay.Frames.Add(new CatchReplayFrame(time - 1, position, false, null));

            var last = currentFrame;
            currentFrame = new CatchReplayFrame(time, position, dashing, last);
            Replay.Frames.Add(currentFrame);
        }
    }
}
