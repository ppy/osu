// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModBlinds : ModBlinds<OsuHitObject>
    {
        public override double ScoreMultiplier => 1.12;
        private DrawableOsuBlinds flashlight;

        public override void ApplyToRulesetContainer(RulesetContainer<OsuHitObject> rulesetContainer)
        {
            rulesetContainer.Overlays.Add(flashlight = new DrawableOsuBlinds(restrictTo: rulesetContainer.Playfield));
        }

        public override void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            scoreProcessor.Health.ValueChanged += val => {
                flashlight.Value = (float)val;
            };
        }
    }
}
