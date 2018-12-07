// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModBlinds : Mod, IApplicableToRulesetContainer<OsuHitObject>, IApplicableToScoreProcessor
    {
        public override string Name => "Blinds";
        public override string Acronym => "BL";
        public override FontAwesome Icon => FontAwesome.fa_adjust;
        public override ModType Type => ModType.DifficultyIncrease;
        public override string Description => "Play with blinds on your screen.";
        public override bool Ranked => false;

        public override double ScoreMultiplier => 1.12;
        private DrawableOsuBlinds flashlight;

        public void ApplyToRulesetContainer(RulesetContainer<OsuHitObject> rulesetContainer)
        {
            bool hasEasy = rulesetContainer.ActiveMods.Any(m => m is ModEasy);
            bool hasHardrock = rulesetContainer.ActiveMods.Any(m => m is ModHardRock);

            rulesetContainer.Overlays.Add(flashlight = new DrawableOsuBlinds(rulesetContainer.Playfield.HitObjectContainer, hasEasy, hasHardrock, rulesetContainer.Beatmap));
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            scoreProcessor.Health.ValueChanged += val => { flashlight.AnimateTarget((float)val); };
        }
    }
}
