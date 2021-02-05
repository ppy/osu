// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModClassic : Mod, IApplicableToHitObject, IApplicableToDrawableHitObjects, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Classic";

        public override string Acronym => "CL";

        public override double ScoreMultiplier => 1;

        public override IconUsage? Icon => FontAwesome.Solid.History;

        public override string Description => "Feeling nostalgic?";

        public override bool Ranked => false;

        [SettingSource("Disable slider head judgement", "Scores sliders proportionally to the number of ticks hit.")]
        public Bindable<bool> DisableSliderHeadJudgement { get; } = new BindableBool(true);

        [SettingSource("Disable slider head tracking", "Pins slider heads at their starting position, regardless of time.")]
        public Bindable<bool> DisableSliderHeadTracking { get; } = new BindableBool(true);

        [SettingSource("Disable note lock lenience", "Applies note lock to the full hit window.")]
        public Bindable<bool> DisableLenientNoteLock { get; } = new BindableBool(true);

        [SettingSource("Disable exact slider follow circle tracking", "Makes the slider follow circle track its final size at all times.")]
        public Bindable<bool> DisableExactFollowCircleTracking { get; } = new BindableBool(true);

        public void ApplyToHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Slider slider:
                    slider.IgnoreJudgement = !DisableSliderHeadJudgement.Value;

                    foreach (var head in slider.NestedHitObjects.OfType<SliderHeadCircle>())
                        head.JudgeAsNormalHitCircle = !DisableSliderHeadJudgement.Value;

                    break;
            }
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            var osuRuleset = (DrawableOsuRuleset)drawableRuleset;

            if (!DisableLenientNoteLock.Value)
                osuRuleset.Playfield.HitPolicy = new ObjectOrderedHitPolicy();
        }

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var obj in drawables)
            {
                switch (obj)
                {
                    case DrawableSlider slider:
                        slider.Ball.TrackVisualSize = !DisableExactFollowCircleTracking.Value;
                        break;

                    case DrawableSliderHead head:
                        head.TrackFollowCircle = !DisableSliderHeadTracking.Value;
                        break;
                }
            }
        }
    }
}
