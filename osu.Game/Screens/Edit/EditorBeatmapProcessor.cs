// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit
{
    public class EditorBeatmapProcessor : IBeatmapProcessor
    {
        public EditorBeatmap Beatmap { get; }

        IBeatmap IBeatmapProcessor.Beatmap => Beatmap;

        private readonly IBeatmapProcessor? rulesetBeatmapProcessor;

        /// <summary>
        /// Kept for the purposes of reducing redundant regeneration of automatic breaks.
        /// </summary>
        private HashSet<(double, double)> objectDurationCache = new HashSet<(double, double)>();

        public EditorBeatmapProcessor(EditorBeatmap beatmap, Ruleset ruleset)
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
            ensureNewComboAfterBreaks();
        }

        private void autoGenerateBreaks()
        {
            var objectDuration = Beatmap.HitObjects.Select(ho => (ho.StartTime - ((ho as IHasTimePreempt)?.TimePreempt ?? 0), ho.GetEndTime())).ToHashSet();

            if (objectDuration.SetEquals(objectDurationCache))
                return;

            objectDurationCache = objectDuration;

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
                var previousObject = Beatmap.HitObjects[i - 1];
                var nextObject = Beatmap.HitObjects[i];

                // Keep track of the maximum end time encountered thus far.
                // This handles cases like osu!mania's hold notes, which could have concurrent other objects after their start time.
                // Note that we're relying on the implicit assumption that objects are sorted by start time,
                // which is why similar tracking is not done for start time.
                currentMaxEndTime = Math.Max(currentMaxEndTime, previousObject.GetEndTime());

                if (nextObject.StartTime - currentMaxEndTime < BreakPeriod.MIN_GAP_DURATION)
                    continue;

                double breakStartTime = currentMaxEndTime + BreakPeriod.GAP_BEFORE_BREAK;

                double breakEndTime = nextObject.StartTime;

                if (nextObject is IHasTimePreempt hasTimePreempt)
                    breakEndTime -= hasTimePreempt.TimePreempt;
                else
                    breakEndTime -= Math.Max(BreakPeriod.GAP_AFTER_BREAK, Beatmap.ControlPointInfo.TimingPointAt(nextObject.StartTime).BeatLength * 2);

                if (breakEndTime - breakStartTime < BreakPeriod.MIN_BREAK_DURATION)
                    continue;

                var breakPeriod = new BreakPeriod(breakStartTime, breakEndTime);

                if (Beatmap.Breaks.Any(b => b.Intersects(breakPeriod)))
                    continue;

                Beatmap.Breaks.Add(breakPeriod);
            }
        }

        private void ensureNewComboAfterBreaks()
        {
            var breakEnds = Beatmap.Breaks.Select(b => b.EndTime).OrderBy(t => t).ToList();

            if (breakEnds.Count == 0)
                return;

            int currentBreak = 0;

            IHasComboInformation? lastObj = null;
            bool comboInformationUpdateRequired = false;

            foreach (var hitObject in Beatmap.HitObjects)
            {
                if (hitObject is not IHasComboInformation hasCombo)
                    continue;

                if (currentBreak < breakEnds.Count && hitObject.StartTime >= breakEnds[currentBreak])
                {
                    if (!hasCombo.NewCombo)
                    {
                        hasCombo.NewCombo = true;
                        comboInformationUpdateRequired = true;
                    }

                    currentBreak += 1;
                }

                if (comboInformationUpdateRequired)
                    hasCombo.UpdateComboInformation(lastObj);

                lastObj = hasCombo;
            }
        }
    }
}
