// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Replays;
using osu.Game.Users;

namespace osu.Game.Rulesets.Taiko.Replays
{
    public class TaikoAutoGenerator : AutoGenerator<TaikoHitObject>
    {
        private const double swell_hit_speed = 50;

        public TaikoAutoGenerator(Beatmap<TaikoHitObject> beatmap)
            : base(beatmap)
        {
            Replay = new Replay
            {
                User = new User
                {
                    Username = @"Autoplay",
                }
            };
        }

        protected Replay Replay;
        protected List<ReplayFrame> Frames => Replay.Frames;

        public override Replay Generate()
        {
            bool hitButton = true;

            Frames.Add(new ReplayFrame(-100000, null, null, ReplayButtonState.None));
            Frames.Add(new ReplayFrame(Beatmap.HitObjects[0].StartTime - 1000, null, null, ReplayButtonState.None));

            for (int i = 0; i < Beatmap.HitObjects.Count; i++)
            {
                TaikoHitObject h = Beatmap.HitObjects[i];

                ReplayButtonState button;

                IHasEndTime endTimeData = h as IHasEndTime;
                double endTime = endTimeData?.EndTime ?? h.StartTime;

                Swell swell = h as Swell;
                DrumRoll drumRoll = h as DrumRoll;
                Hit hit = h as Hit;

                if (swell != null)
                {
                    int d = 0;
                    int count = 0;
                    int req = swell.RequiredHits;
                    double hitRate = Math.Min(swell_hit_speed, swell.Duration / req);
                    for (double j = h.StartTime; j < endTime; j += hitRate)
                    {
                        switch (d)
                        {
                            default:
                            case 0:
                                button = ReplayButtonState.Left1;
                                break;
                            case 1:
                                button = ReplayButtonState.Right1;
                                break;
                            case 2:
                                button = ReplayButtonState.Left2;
                                break;
                            case 3:
                                button = ReplayButtonState.Right2;
                                break;
                        }

                        Frames.Add(new ReplayFrame(j, null, null, button));
                        d = (d + 1) % 4;
                        if (++count == req)
                            break;
                    }
                }
                else if (drumRoll != null)
                {
                    foreach (var tick in drumRoll.Ticks)
                    {
                        Frames.Add(new ReplayFrame(tick.StartTime, null, null, hitButton ? ReplayButtonState.Left1 : ReplayButtonState.Left2));
                        hitButton = !hitButton;
                    }
                }
                else if (hit != null)
                {
                    if (hit is CentreHit)
                    {
                        if (h.IsStrong)
                            button = ReplayButtonState.Right1 | ReplayButtonState.Right2;
                        else
                            button = hitButton ? ReplayButtonState.Right1 : ReplayButtonState.Right2;
                    }
                    else
                    {
                        if (h.IsStrong)
                            button = ReplayButtonState.Left1 | ReplayButtonState.Left2;
                        else
                            button = hitButton ? ReplayButtonState.Left1 : ReplayButtonState.Left2;
                    }

                    Frames.Add(new ReplayFrame(h.StartTime, null, null, button));
                }
                else
                    throw new InvalidOperationException("Unknown hit object type.");

                Frames.Add(new ReplayFrame(endTime + KEY_UP_DELAY, null, null, ReplayButtonState.None));

                if (i < Beatmap.HitObjects.Count - 1)
                {
                    double waitTime = Beatmap.HitObjects[i + 1].StartTime - 1000;
                    if (waitTime > endTime)
                        Frames.Add(new ReplayFrame(waitTime, null, null, ReplayButtonState.None));
                }

                hitButton = !hitButton;
            }

            return Replay;
        }
    }
}
