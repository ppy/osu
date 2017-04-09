// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
using osu.Game.Modes.Objects.Types;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.Replays;

namespace osu.Game.Modes.Taiko.Replays
{
    public class TaikoAutoReplay : Replay
    {
        private readonly Beatmap<TaikoHitObject> beatmap;

        public TaikoAutoReplay(Beatmap<TaikoHitObject> beatmap)
        {
            this.beatmap = beatmap;

            createAutoReplay();
        }

        private void createAutoReplay()
        {
            bool hitButton = true;

            Frames.Add(new ReplayFrame(-100000, 320, 240, ReplayButtonState.None));
            Frames.Add(new ReplayFrame(beatmap.HitObjects[0].StartTime - 1000, 320, 240, ReplayButtonState.None));

            for (int i = 0; i < beatmap.HitObjects.Count; i++)
            {
                TaikoHitObject h = beatmap.HitObjects[i];

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
                    double hitRate = swell.Duration / req;
                    for (double j = h.StartTime; j < endTime; j += hitRate)
                    {
                        switch (d)
                        {
                            default:
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

                        Frames.Add(new ReplayFrame(j, 0, 0, button));
                        d = (d + 1) % 4;
                        if (++count > req)
                            break;
                    }
                }
                else if (drumRoll != null)
                {
                    foreach (var tick in drumRoll.Ticks)
                    {
                        Frames.Add(new ReplayFrame(tick.StartTime, 0, 0, hitButton ? ReplayButtonState.Left1 : ReplayButtonState.Left2));
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

                    Frames.Add(new ReplayFrame(h.StartTime, 0, 0, button));
                }
                else
                    throw new Exception("Unknown hit object type.");

                Frames.Add(new ReplayFrame(endTime + 1, 0, 0, ReplayButtonState.None));

                if (i < beatmap.HitObjects.Count - 1)
                {
                    double waitTime = beatmap.HitObjects[i + 1].StartTime - 1000;
                    if (waitTime > endTime)
                        Frames.Add(new ReplayFrame(waitTime, 0, 0, ReplayButtonState.None));
                }

                hitButton = !hitButton;
            }
        }
    }
}