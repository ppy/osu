// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.Objects.Types;
using osu.Game.Beatmaps;

namespace osu.Game.Modes.Taiko
{
    public class TaikoAutoReplay : LegacyTaikoReplay
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

            Frames.Add(new LegacyReplayFrame(-100000, 320, 240, LegacyButtonState.None));
            Frames.Add(new LegacyReplayFrame(beatmap.HitObjects[0].StartTime - 1000, 320, 240, LegacyButtonState.None));

            for (int i = 0; i < beatmap.HitObjects.Count; i++)
            {
                TaikoHitObject h = beatmap.HitObjects[i];

                LegacyButtonState button;

                IHasEndTime endTimeData = h as IHasEndTime;
                double endTime = endTimeData?.EndTime ?? h.StartTime;

                Swell sp = h as Swell;
                if (sp != null)
                {
                    int d = 0;
                    int count = 0;
                    int req = sp.RequiredHits;
                    double hitRate = sp.Duration / req;
                    for (double j = h.StartTime; j < endTime; j += hitRate)
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
                    Hit hit = h as Hit;

                    if (hit.Type == HitType.Centre)
                    {
                        if (h.IsStrong)
                            button = LegacyButtonState.Right1 | LegacyButtonState.Right2;
                        else
                            button = hitButton ? LegacyButtonState.Right1 : LegacyButtonState.Right2;
                    }
                    else
                    {
                        if (h.IsStrong)
                            button = LegacyButtonState.Left1 | LegacyButtonState.Left2;
                        else
                            button = hitButton ? LegacyButtonState.Left1 : LegacyButtonState.Left2;
                    }

                    Frames.Add(new LegacyReplayFrame(h.StartTime, 0, 0, button));
                }

                Frames.Add(new LegacyReplayFrame(endTime + 1, 0, 0, LegacyButtonState.None));

                if (i < beatmap.HitObjects.Count - 1)
                {
                    double waitTime = beatmap.HitObjects[i + 1].StartTime - 1000;
                    if (waitTime > endTime)
                        Frames.Add(new LegacyReplayFrame(waitTime, 0, 0, LegacyButtonState.None));
                }

                hitButton = !hitButton;
            }

            //Player.currentScore.Replay = InputManager.ReplayScore.Replay;
            //Player.currentScore.PlayerName = "mekkadosu!";
        }
    }
}