// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using System.Linq;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModBlinds : ModBlinds<OsuHitObject>
    {
        public override double ScoreMultiplier => 1.12;
        private DrawableOsuBlinds flashlight;

        public override void ApplyToRulesetContainer(RulesetContainer<OsuHitObject> rulesetContainer)
        {
            bool hasEasy = rulesetContainer.ActiveMods.Count(mod => mod is ModEasy) > 0;
            rulesetContainer.Overlays.Add(flashlight = new DrawableOsuBlinds(restrictTo: rulesetContainer.Playfield, hasEasy: hasEasy));
        }

        public override void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            scoreProcessor.Health.ValueChanged += val => {
                flashlight.Value = (float)val;
            };
        }
    }
}
