// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Objects;

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
            if (Beatmap.HitObjects.Count == 0)
                return Replay;

            bool hitButton = true;

            Frames.Add(new TaikoReplayFrame(-100000));
            Frames.Add(new TaikoReplayFrame(Beatmap.HitObjects[0].StartTime - 1000));

            for (int i = 0; i < Beatmap.HitObjects.Count; i++)
            {
                TaikoHitObject h = Beatmap.HitObjects[i];
                double endTime = h.GetEndTime();

                switch (h)
                {
                    case Swell swell:
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

                        break;
                    }

                    case DrumRoll drumRoll:
                    {
                        foreach (var tick in drumRoll.NestedHitObjects.OfType<DrumRollTick>())
                        {
                            Frames.Add(new TaikoReplayFrame(tick.StartTime, hitButton ? TaikoAction.LeftCentre : TaikoAction.RightCentre));
                            hitButton = !hitButton;
                        }

                        break;
                    }

                    case Hit hit:
                    {
                        TaikoAction[] actions;

                        if (hit.Type == HitType.Centre)
                        {
                            actions = hit.IsStrong
                                ? new[] { TaikoAction.LeftCentre, TaikoAction.RightCentre }
                                : new[] { hitButton ? TaikoAction.LeftCentre : TaikoAction.RightCentre };
                        }
                        else
                        {
                            actions = hit.IsStrong
                                ? new[] { TaikoAction.LeftRim, TaikoAction.RightRim }
                                : new[] { hitButton ? TaikoAction.LeftRim : TaikoAction.RightRim };
                        }

                        Frames.Add(new TaikoReplayFrame(h.StartTime, actions));
                        break;
                    }

                    default:
                        throw new InvalidOperationException("Unknown hit object type.");
                }

                var nextHitObject = GetNextObject(i); // Get the next object that requires pressing the same button

                bool canDelayKeyUp = nextHitObject == null || nextHitObject.StartTime > endTime + KEY_UP_DELAY;
                double calculatedDelay = canDelayKeyUp ? KEY_UP_DELAY : (nextHitObject.StartTime - endTime) * 0.9;
                Frames.Add(new TaikoReplayFrame(endTime + calculatedDelay));

                hitButton = !hitButton;
            }

            return Replay;
        }
    }
}
