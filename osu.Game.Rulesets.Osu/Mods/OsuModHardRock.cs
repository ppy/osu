// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModHardRock : ModHardRock, IApplicableToHitObject
    {
        public override double ScoreMultiplier => 1.06;
        public override bool Ranked => true;

        public void ApplyToHitObject(HitObject hitObject)
        {
            var osuObject = (OsuHitObject)hitObject;

            osuObject.Position = new Vector2(osuObject.Position.X, OsuPlayfield.BASE_SIZE.Y - osuObject.Y);

            var slider = hitObject as Slider;
            if (slider == null)
                return;

            slider.NestedHitObjects.OfType<SliderTick>().ForEach(h => h.Position = new Vector2(h.Position.X, OsuPlayfield.BASE_SIZE.Y - h.Position.Y));
            slider.NestedHitObjects.OfType<RepeatPoint>().ForEach(h => h.Position = new Vector2(h.Position.X, OsuPlayfield.BASE_SIZE.Y - h.Position.Y));

            var newSegments = new PathSegment[slider.Path.Segments.Length];

            for (int i = 0; i < slider.Path.Segments.Length; i++)
            {
                var newControlPoints = slider.Path.Segments[i].ControlPoints.ToArray();

                for (int j = 0; j < newControlPoints.Length; j++)
                    newControlPoints[j].Y = -newControlPoints[j].Y;

                newSegments[i] = new PathSegment(slider.Path.Segments[i].Type, newControlPoints);
            }

            slider.Path = new SliderPath(newSegments, slider.Path.ExpectedDistance);
        }
    }
}
