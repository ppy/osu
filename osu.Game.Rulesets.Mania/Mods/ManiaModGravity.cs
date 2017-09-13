// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Timing;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.Mania.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModGravity : Mod, IGenerateSpeedAdjustments
    {
        public override string Name => "Gravity";
        public override string ShortenedName => "GR";

        public override double ScoreMultiplier => 0;

        public override FontAwesome Icon => FontAwesome.fa_sort_desc;

        public void ApplyToRulesetContainer(ManiaRulesetContainer rulesetContainer, ref List<SpeedAdjustmentContainer>[] hitObjectTimingChanges, ref List<SpeedAdjustmentContainer> barlineTimingChanges)
        {
            // We have to generate one speed adjustment per hit object for gravity
            foreach (ManiaHitObject obj in rulesetContainer.Objects.OfType<ManiaHitObject>())
            {
                MultiplierControlPoint controlPoint = rulesetContainer.CreateControlPointAt(obj.StartTime);
                // Beat length has too large of an effect for gravity, so we'll force it to a constant value for now
                controlPoint.TimingPoint.BeatLength = 1000;

                hitObjectTimingChanges[obj.Column].Add(new ManiaSpeedAdjustmentContainer(controlPoint, ScrollingAlgorithm.Gravity));
            }

            // Like with hit objects, we need to generate one speed adjustment per bar line
            foreach (DrawableBarLine barLine in rulesetContainer.BarLines)
            {
                var controlPoint = rulesetContainer.CreateControlPointAt(barLine.HitObject.StartTime);
                // Beat length has too large of an effect for gravity, so we'll force it to a constant value for now
                controlPoint.TimingPoint.BeatLength = 1000;

                barlineTimingChanges.Add(new ManiaSpeedAdjustmentContainer(controlPoint, ScrollingAlgorithm.Gravity));
            }
        }
    }
}