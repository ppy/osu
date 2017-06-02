using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Mania.Timing;
using osu.Game.Rulesets.Mania.Timing.Drawables;
using osu.Game.Rulesets.Objects.Types;
using System.Linq;
using osu.Framework.Lists;
using osu.Game.Beatmaps.ControlPoints;
using osu.Framework.MathUtils;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModGravity : Mod, IApplicableMod<ManiaHitObject>
    {
        public override string Name => "Gravity";

        public override double ScoreMultiplier => 0;

        public override FontAwesome Icon => FontAwesome.fa_sort_desc;

        public void ApplyToHitRenderer(HitRenderer<ManiaHitObject> hitRenderer)
        {
            var maniaHitRenderer = (ManiaHitRenderer)hitRenderer;

            maniaHitRenderer.HitObjectTimingChanges = new Dictionary<int, List<DrawableTimingChange>>();
            maniaHitRenderer.BarlineTimingChanges = new List<DrawableTimingChange>();

            foreach (ManiaHitObject obj in maniaHitRenderer.Objects)
            {
                List<DrawableTimingChange> timingChanges;
                if (!maniaHitRenderer.HitObjectTimingChanges.TryGetValue(obj.Column, out timingChanges))
                    maniaHitRenderer.HitObjectTimingChanges[obj.Column] = timingChanges = new List<DrawableTimingChange>();

                timingChanges.Add(new DrawableGravityTimingChange(new TimingChange
                {
                    Time = obj.StartTime,
                    BeatLength = 1000
                }));
            }

            double lastObjectTime = (maniaHitRenderer.Objects.LastOrDefault() as IHasEndTime)?.EndTime ?? maniaHitRenderer.Objects.LastOrDefault()?.StartTime ?? double.MaxValue;

            SortedList<TimingControlPoint> timingPoints = maniaHitRenderer.Beatmap.ControlPointInfo.TimingPoints;
            for (int i = 0; i < timingPoints.Count; i++)
            {
                TimingControlPoint point = timingPoints[i];

                // Stop on the beat before the next timing point, or if there is no next timing point stop slightly past the last object
                double endTime = i < timingPoints.Count - 1 ? timingPoints[i + 1].Time - point.BeatLength : lastObjectTime + point.BeatLength * (int)point.TimeSignature;

                int index = 0;
                for (double t = timingPoints[i].Time; Precision.DefinitelyBigger(endTime, t); t += point.BeatLength, index++)
                {
                    maniaHitRenderer.BarlineTimingChanges.Add(new DrawableGravityTimingChange(new TimingChange
                    {
                        Time = t,
                        BeatLength = 1000
                    }));
                }
            }
        }
    }
}