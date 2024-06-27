// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit
{
    public class EditorBeatmapProcessor : IBeatmapProcessor
    {
        public IBeatmap Beatmap { get; }

        private readonly IBeatmapProcessor? rulesetBeatmapProcessor;

        public EditorBeatmapProcessor(IBeatmap beatmap, Ruleset ruleset)
        {
            Beatmap = beatmap;
            rulesetBeatmapProcessor = ruleset.CreateBeatmapProcessor(beatmap);
        }

        public void PreProcess()
        {
            rulesetBeatmapProcessor?.PreProcess();
        }

        public void PostProcess()
        {
            rulesetBeatmapProcessor?.PostProcess();

            autoGenerateBreaks();
        }

        private void autoGenerateBreaks()
        {
            Beatmap.Breaks.RemoveAll(b => b is not ManualBreakPeriod);

            foreach (var manualBreak in Beatmap.Breaks.ToList())
            {
                if (manualBreak.EndTime <= Beatmap.HitObjects.FirstOrDefault()?.StartTime
                    || manualBreak.StartTime >= Beatmap.GetLastObjectTime()
                    || Beatmap.HitObjects.Any(ho => ho.StartTime <= manualBreak.EndTime && ho.GetEndTime() >= manualBreak.StartTime))
                {
                    Beatmap.Breaks.Remove(manualBreak);
                }
            }

            double currentMaxEndTime = double.MinValue;

            for (int i = 1; i < Beatmap.HitObjects.Count; ++i)
            {
                // Keep track of the maximum end time encountered thus far.
                // This handles cases like osu!mania's hold notes, which could have concurrent other objects after their start time.
                // Note that we're relying on the implicit assumption that objects are sorted by start time,
                // which is why similar tracking is not done for start time.
                currentMaxEndTime = Math.Max(currentMaxEndTime, Beatmap.HitObjects[i - 1].GetEndTime());

                double nextObjectStartTime = Beatmap.HitObjects[i].StartTime;

                if (nextObjectStartTime - currentMaxEndTime < BreakPeriod.MIN_GAP_DURATION)
                    continue;

                double breakStartTime = currentMaxEndTime + BreakPeriod.GAP_BEFORE_BREAK;
                double breakEndTime = nextObjectStartTime - Math.Max(BreakPeriod.GAP_AFTER_BREAK, Beatmap.ControlPointInfo.TimingPointAt(nextObjectStartTime).BeatLength * 2);

                if (breakEndTime - breakStartTime < BreakPeriod.MIN_BREAK_DURATION)
                    continue;

                var breakPeriod = new BreakPeriod(breakStartTime, breakEndTime);

                if (Beatmap.Breaks.Any(b => b.Intersects(breakPeriod)))
                    continue;

                Beatmap.Breaks.Add(breakPeriod);
            }
        }
    }
}
