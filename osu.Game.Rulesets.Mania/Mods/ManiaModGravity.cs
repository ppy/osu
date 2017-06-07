// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Mania.Timing.Drawables;
using osu.Game.Rulesets.Objects.Types;
using System.Linq;
using osu.Framework.Lists;
using osu.Game.Beatmaps.ControlPoints;
using osu.Framework.MathUtils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.Timing.Drawables;

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

            maniaHitRenderer.HitObjectTimingChanges = new List<DrawableTimingChange>[maniaHitRenderer.PreferredColumns];
            maniaHitRenderer.BarlineTimingChanges = new List<DrawableTimingChange>();

            for (int i = 0; i < maniaHitRenderer.PreferredColumns; i++)
                maniaHitRenderer.HitObjectTimingChanges[i] = new List<DrawableTimingChange>();

            foreach (HitObject obj in maniaHitRenderer.Objects)
            {
                var maniaObject = obj as ManiaHitObject;
                if (maniaObject == null)
                    continue;

                maniaHitRenderer.HitObjectTimingChanges[maniaObject.Column].Add(new DrawableManiaGravityTimingChange(new TimingChange
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

                for (double t = timingPoints[i].Time; Precision.DefinitelyBigger(endTime, t); t += point.BeatLength)
                {
                    maniaHitRenderer.BarlineTimingChanges.Add(new DrawableManiaGravityTimingChange(new TimingChange
                    {
                        Time = t,
                        BeatLength = 1000
                    }));
                }
            }
        }
    }
}