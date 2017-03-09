﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Modes.Taiko.Objects;

namespace osu.Game.Modes.Taiko.Replays
{
    public class AutoReplay : LegacyTaikoReplay
    {
        private List<TaikoHitObject> hitObjects;

        public AutoReplay(List<TaikoHitObject> hitObjects)
        {
            this.hitObjects = hitObjects;

            createAutoReplay();
        }

        private void createAutoReplay()
        {
            bool hitButton = true;

            Frames.Add(new LegacyReplayFrame(-100000, 320, 240, LegacyButtonState.None));
            Frames.Add(new LegacyReplayFrame(hitObjects[0].StartTime - 1000, 320, 240, LegacyButtonState.None));

            for (int i = 0; i < hitObjects.Count; i++)
            {
                TaikoHitObject h = hitObjects[i];

                LegacyButtonState button;

                Bash sp = h as Bash;
                if (sp != null)
                {
                    int d = 0;
                    int count = 0;
                    int req = sp.RequiredHits;
                    double hitRate = sp.Duration / req;
                    for (double j = h.StartTime; j < h.EndTime; j += hitRate)
                    {
                        switch (d)
                        {
                            default:
                                button = LegacyButtonState.Left1;
                                break;
                            case 1:
                                button = LegacyButtonState.Right1;
                                break;
                            case 2:
                                button = LegacyButtonState.Left2;
                                break;
                            case 3:
                                button = LegacyButtonState.Right2;
                                break;
                        }

                        Frames.Add(new LegacyReplayFrame(j, 0, 0, button));
                        d = (d + 1) % 4;
                        if (++count > req)
                            break;
                    }
                }
                else if (h is DrumRoll)
                {
                    DrumRoll d = h as DrumRoll;


                    double delay = d.TickTimeDistance;

                    double time = d.StartTime;

                    for (int j = 0; j < d.TotalTicks; j++)
                    {
                        Frames.Add(new LegacyReplayFrame((int)time, 0, 0, hitButton ? LegacyButtonState.Left1 : LegacyButtonState.Left2));
                        time += delay;
                        hitButton = !hitButton;
                    }
                }
                else
                {
                    if ((h.Type & TaikoHitType.CentreHit) > 0)
                    {
                        if ((h.Type & TaikoHitType.Finisher) > 0)
                            button = LegacyButtonState.Right1 | LegacyButtonState.Right2;
                        else
                            button = hitButton ? LegacyButtonState.Right1 : LegacyButtonState.Right2;
                    }
                    else
                    {
                        if ((h.Type & TaikoHitType.Finisher) > 0)
                            button = LegacyButtonState.Left1 | LegacyButtonState.Left2;
                        else
                            button = hitButton ? LegacyButtonState.Left1 : LegacyButtonState.Left2;
                    }

                    Frames.Add(new LegacyReplayFrame(h.StartTime, 0, 0, button));
                }

                Frames.Add(new LegacyReplayFrame(h.EndTime + 1, 0, 0, LegacyButtonState.None));

                if (i < hitObjects.Count - 1)
                {
                    double waitTime = hitObjects[i + 1].StartTime - 1000;
                    if (waitTime > h.EndTime)
                        Frames.Add(new LegacyReplayFrame(waitTime, 0, 0, LegacyButtonState.None));
                }

                hitButton = !hitButton;
            }

            //Player.currentScore.Replay = InputManager.ReplayScore.Replay;
            //Player.currentScore.PlayerName = "mekkadosu!";
        }
    }
}
