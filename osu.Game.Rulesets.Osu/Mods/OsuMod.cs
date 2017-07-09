// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using System;
using System.Linq;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModNoFail : ModNoFail
    {
        public override string Name => "NoFail";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_easy;
        public override string Description => @"You can't fail. No matter what.";
        public override double ScoreMultiplier => 0.5;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModSuddenDeath), typeof(OsuModPerfect), typeof(ModRelax), typeof(OsuModAutopilot), typeof(OsuModAutoplay) };
    }

    public class OsuModEasy : ModEasy
    {
        public override string Name => "Easy";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_easy;
        public override string Description => @"Reduces overall difficulty - larger circles, more forgiving HP drain, less accuracy required.";
        public override double ScoreMultiplier => 0.5;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModHardRock) }).ToArray();
    }

    public class OsuModHidden : ModHidden
    {
        public override string Name => "Hidden";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_hidden;
        public override string Description => @"Play with no approach circles and fading notes for a slight score advantage.";
        public override double ScoreMultiplier => 1.06;
        public override bool Ranked => true;
    }

    public class OsuModHardRock : ModHardRock
    {
        public override string Name => "Hard Rock";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_hardrock;
        public override string Description => @"Everything just got a bit harder...";
        public override double ScoreMultiplier => 1.06;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModEasy) }).ToArray();
    }

    public class OsuModSuddenDeath : ModSuddenDeath
    {
        public override string Name => "Sudden Death";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_suddendeath;
        public override string Description => @"Miss a note and fail.";
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof (OsuModNoFail), typeof(OsuModRelax), typeof(OsuModAutoplay), typeof (OsuModAutopilot), };
    }

    public class OsuModDaycore : ModDaycore
    {
        public override string Name => "Daycore";
     // public override FontAwesome Icon => FontAwesome.fa_osu_mod_daycore;
        public override string Description => @"whoaaaaa";
        public override double ScoreMultiplier => 0.3;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModDoubleTime), typeof(OsuModNightcore) };
    }

    public class OsuModDoubleTime : ModDoubleTime
    {
        public override string Name => "Double Time";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_doubletime;
        public override string Description => @"Zoooooooooom.";
        public override double ScoreMultiplier => 1.12;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModHalfTime), typeof(OsuModDaycore) };
    }

    public class OsuModRelax : ModRelax
    {
        public override string Name => "Relax";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_relax;
        public override string Description => @"You don't need to click.\nGive your clicking/tapping finger a break from the heat of things.";
        public override double ScoreMultiplier => 0;
        public override bool Ranked => false;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(OsuModAutopilot) };
    }

    public class OsuModHalfTime : ModHalfTime
    {
        public override string Name => "Half Time";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_halftime;
        public override string Description => @"Less zoom.";
        public override double ScoreMultiplier => 0.3;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModDoubleTime), typeof(OsuModNightcore) };
    }

    public class OsuModNightcore : ModNightcore
    {
        public override string Name => "Nightcore";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_nightcore;
        public override string Description => @"uguuuuuuuu";
        public override double ScoreMultiplier => 1.12;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModHalfTime), typeof(OsuModDaycore) };
    }

    public class OsuModFlashlight : ModFlashlight
    {
        public override string Name => "Flashlight";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_flashlight;
        public override string Description => @"Restricted view area.";
        public override double ScoreMultiplier => 1.12;
        public override bool Ranked => true;
    }

    public class OsuModPerfect : ModPerfect
    {
        public override string Name => "Perfect";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_perfect;
        public override string Description => @"SS or quit.";
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModNoFail), typeof(OsuModRelax), typeof(OsuModAutoplay), typeof(OsuModAutopilot), };
    }

    public class OsuModSpunOut : Mod
    {
        public override string Name => "Spun Out";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_spunout;
        public override string Description => @"Spinners will be automatically completed";
        public override double ScoreMultiplier => 0.9;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(OsuModAutopilot) };
    }

    public class OsuModAutopilot : Mod
    {
        public override string Name => "Autopilot";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_autopilot;
        public override string Description => @"Automatic cursor movement - just follow the rhythm.";
        public override double ScoreMultiplier => 0;
        public override bool Ranked => false;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModSpunOut), typeof(ModRelax), typeof(ModSuddenDeath), typeof(ModNoFail), typeof(ModAutoplay), typeof(ModPerfect) };
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
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_target;
        public override string Description => @"Train your BPM consistency!";
        public override double ScoreMultiplier => 1;
    }
}
