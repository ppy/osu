// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.Mania.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModGravity : Mod, IGenerateSpeedAdjustments
    {
        public override string Name => "Gravity";

        public override double ScoreMultiplier => 0;

        public override FontAwesome Icon => FontAwesome.fa_sort_desc;

        public void ApplyToHitRenderer(ManiaHitRenderer hitRenderer, ref List<SpeedAdjustmentContainer>[] hitObjectTimingChanges, ref List<SpeedAdjustmentContainer> barlineTimingChanges)
        {
            foreach (HitObject obj in hitRenderer.Objects)
            {
                var maniaObject = obj as ManiaHitObject;
                if (maniaObject == null)
                    continue;

                MultiplierControlPoint controlPoint = hitRenderer.CreateControlPointAt(obj.StartTime);
                controlPoint.TimingPoint.BeatLength = 1000;

                hitObjectTimingChanges[maniaObject.Column].Add(new ManiaSpeedAdjustmentContainer(controlPoint, ScrollingAlgorithm.Gravity));
            }

            foreach (DrawableBarLine barLine in hitRenderer.BarLines)
            {
                var controlPoint = hitRenderer.CreateControlPointAt(barLine.HitObject.StartTime);
                controlPoint.TimingPoint.BeatLength = 1000;

                barlineTimingChanges.Add(new ManiaSpeedAdjustmentContainer(controlPoint, ScrollingAlgorithm.Gravity));
            }
        }
    }
}