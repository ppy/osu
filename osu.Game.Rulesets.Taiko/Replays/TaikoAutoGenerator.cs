// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Taiko.Beatmaps;

namespace osu.Game.Rulesets.Taiko.Replays
{
    public class TaikoAutoGenerator : AutoGenerator
    {
        public new TaikoBeatmap Beatmap => (TaikoBeatmap)base.Beatmap;

        private const double swell_hit_speed = 50;

        public TaikoAutoGenerator(IBeatmap beatmap)
            : base(beatmap)
        {
            Replay = new Replay();
        }

        protected Replay Replay;
        protected List<ReplayFrame> Frames => Replay.Frames;

        public override Replay Generate()
        {
            bool hitButton = true;

            Frames.Add(new TaikoReplayFrame(-100000));
            Frames.Add(new TaikoReplayFrame(Beatmap.HitObjects[0].StartTime - 1000));

            for (int i = 0; i < Beatmap.HitObjects.Count; i++)
            {
                TaikoHitObject h = Beatmap.HitObjects[i];

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
                        TaikoAction action;

                        switch (d)
                        {
                            default:
                            case 0:
                                action = TaikoAction.LeftCentre;
                                break;
                            case 1:
                                action = TaikoAction.LeftRim;
                                break;
                            case 2:
                                action = TaikoAction.RightCentre;
                                break;
                            case 3:
                                action = TaikoAction.RightRim;
                                break;
                        }

                        Frames.Add(new TaikoReplayFrame(j, action));
                        d = (d + 1) % 4;
                        if (++count == req)
                            break;
                    }
                }
                else if (drumRoll != null)
                {
                    foreach (var tick in drumRoll.NestedHitObjects.OfType<DrumRollTick>())
                    {
                        Frames.Add(new TaikoReplayFrame(tick.StartTime, hitButton ? TaikoAction.LeftCentre : TaikoAction.RightCentre));
                        hitButton = !hitButton;
                    }
                }
                else if (hit != null)
                {
                    TaikoAction[] actions;

                    if (hit is CentreHit)
                    {
                        actions = h.IsStrong
                            ? new[] { TaikoAction.LeftCentre, TaikoAction.RightCentre }
                            : new[] { hitButton ? TaikoAction.LeftCentre : TaikoAction.RightCentre };
                    }
                    else
                    {
                        actions = h.IsStrong
                            ? new[] { TaikoAction.LeftRim, TaikoAction.RightRim }
                            : new[] { hitButton ? TaikoAction.LeftRim : TaikoAction.RightRim };
                    }

                    Frames.Add(new TaikoReplayFrame(h.StartTime, actions));
                }
                else
                    throw new InvalidOperationException("Unknown hit object type.");

                Frames.Add(new TaikoReplayFrame(endTime + KEY_UP_DELAY));

                if (i < Beatmap.HitObjects.Count - 1)
                {
                    double waitTime = Beatmap.HitObjects[i + 1].StartTime - 1000;
                    if (waitTime > endTime)
                        Frames.Add(new TaikoReplayFrame(waitTime));
                }

                hitButton = !hitButton;
            }

            return Replay;
        }
    }
}
