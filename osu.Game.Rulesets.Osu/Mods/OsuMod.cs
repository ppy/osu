// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModNoFail : ModNoFail
    {
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModAutopilot) }).ToArray();
    }

    public class OsuModEasy : ModEasy
    {

    }

    public class OsuModHidden : ModHidden
    {
        public override string Description => @"Play with no approach circles and fading notes for a slight score advantage.";
        public override double ScoreMultiplier => 1.06;
    }

    public class OsuModHardRock : ModHardRock, IApplicableMod<OsuHitObject>
    {
        public override double ScoreMultiplier => 1.06;
        public override bool Ranked => true;

        public void ApplyToRulesetContainer(RulesetContainer<OsuHitObject> rulesetContainer)
        {
            rulesetContainer.Objects.OfType<OsuHitObject>().ForEach(h => h.Position = new Vector2(h.Position.X, OsuPlayfield.BASE_SIZE.Y - h.Y));
            rulesetContainer.Objects.OfType<Slider>().ForEach(s =>
            {
                var newControlPoints = new List<Vector2>();
                s.ControlPoints.ForEach(c => newControlPoints.Add(new Vector2(c.X, OsuPlayfield.BASE_SIZE.Y - c.Y)));

                s.ControlPoints = newControlPoints;
                s.Curve?.Calculate(); // Recalculate the slider curve
            });
        }
    }

    public class OsuModSuddenDeath : ModSuddenDeath
    {
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModAutopilot) }).ToArray();
    }

    public class OsuModDaycore : ModDaycore
    {
        public override double ScoreMultiplier => 0.5;
    }

    public class OsuModDoubleTime : ModDoubleTime
    {
        public override double ScoreMultiplier => 1.12;
    }

    public class OsuModRelax : ModRelax
    {
        public override string Description => "You don't need to click.\nGive your clicking/tapping finger a break from the heat of things.";
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModAutopilot) }).ToArray();
    }

    public class OsuModHalfTime : ModHalfTime
    {
        public override double ScoreMultiplier => 0.5;
    }

    public class OsuModNightcore : ModNightcore
    {
        public override double ScoreMultiplier => 1.12;
    }

    public class OsuModFlashlight : ModFlashlight
    {
        public override double ScoreMultiplier => 1.12;
    }

    public class OsuModPerfect : ModPerfect
    {

    }

    public class OsuModSpunOut : Mod
    {
        public override string Name => "Spun Out";
        public override string ShortenedName => "SO";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_spunout;
        public override string Description => @"Spinners will be automatically completed";
        public override double ScoreMultiplier => 0.9;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(OsuModAutopilot) };
    }

    public class OsuModAutopilot : Mod
    {
        public override string Name => "Autopilot";
        public override string ShortenedName => "AP";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_autopilot;
        public override string Description => @"Automatic cursor movement - just follow the rhythm.";
        public override double ScoreMultiplier => 0;
        public override bool Ranked => false;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModSpunOut), typeof(ModRelax), typeof(ModSuddenDeath), typeof(ModNoFail), typeof(ModAutoplay) };
    }

    public class OsuModAutoplay : ModAutoplay<OsuHitObject>
    {
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModAutopilot), typeof(OsuModSpunOut) }).ToArray();

        protected override Score CreateReplayScore(Beatmap<OsuHitObject> beatmap) => new Score
        {
            Replay = new OsuAutoGenerator(beatmap).Generate()
        };
    }

    public class OsuModTarget : Mod
    {
        public override string Name => "Target";
        public override string ShortenedName => "TP";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_target;
        public override string Description => @"";
        public override double ScoreMultiplier => 1;
    }
}
