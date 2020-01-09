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
    public class OsuModMirror : ModMirror, IApplicableToHitObject
    {
        public void ApplyToHitObject(HitObject hitObject)
        {
            var osuObject = (OsuHitObject)hitObject;

            osuObject.Position = new Vector2(OsuPlayfield.BASE_SIZE.X - osuObject.Position.X, osuObject.Y);

            if (!(hitObject is Slider slider))
                return;

            slider.NestedHitObjects.OfType<SliderTick>().ForEach(h => h.Position = new Vector2(OsuPlayfield.BASE_SIZE.X - h.Position.X, h.Position.Y));
            slider.NestedHitObjects.OfType<RepeatPoint>().ForEach(h => h.Position = new Vector2(OsuPlayfield.BASE_SIZE.X - h.Position.X, h.Position.Y));

            foreach (var point in slider.Path.ControlPoints)
                point.Position.Value = new Vector2(-point.Position.Value.X, point.Position.Value.Y);
        }
    }
}