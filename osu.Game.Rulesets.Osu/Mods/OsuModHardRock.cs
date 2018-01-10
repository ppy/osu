// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModHardRock : ModHardRock, IApplicableToHitObject<OsuHitObject>
    {
        public override double ScoreMultiplier => 1.06;
        public override bool Ranked => true;

        public void ApplyToHitObject(OsuHitObject hitObject)
        {
            hitObject.Position = new Vector2(hitObject.Position.X, OsuPlayfield.BASE_SIZE.Y - hitObject.Y);

            var slider = hitObject as Slider;
            if (slider == null)
                return;

            var newControlPoints = new List<Vector2>();
            slider.ControlPoints.ForEach(c => newControlPoints.Add(new Vector2(c.X, OsuPlayfield.BASE_SIZE.Y - c.Y)));

            slider.ControlPoints = newControlPoints;
            slider.Curve?.Calculate(); // Recalculate the slider curve
        }
    }
}
