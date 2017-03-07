﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Graphics;
using osu.Game.Screens.Play;

namespace osu.Game.Modes
{
    /// <summary>
    /// The base class for gameplay modifiers.
    /// </summary>
    public abstract class Mod
    {
        /// <summary>
        /// The name of this mod.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The icon of this mod.
        /// </summary>
        public virtual FontAwesome Icon => FontAwesome.fa_question;

        /// <summary>
        /// The user readable description of this mod.
        /// </summary>
        public virtual string Description => string.Empty;

        /// <summary>
        /// The score multiplier of this mod.
        /// </summary>
        public abstract double ScoreMultiplier { get; }

        /// <summary>
        /// Returns if this mod is ranked.
        /// </summary>
        public virtual bool Ranked => false;

        /// <summary>
        /// The mods this mod cannot be enabled with.
        /// </summary>
        public virtual Type[] IncompatibleMods => new Type[] { };

        /// <summary>
        /// Direct access to the Player before load has run.
        /// </summary>
        /// <param name="player"></param>
        public virtual void PlayerLoading(Player player) { }
    }

    public class MultiMod : Mod
    {
        public override string Name => string.Empty;
        public override string Description => string.Empty;
        public override double ScoreMultiplier => 0.0;

        public Mod[] Mods;
    }

    public abstract class ModNoFail : Mod
    {
        public override string Name => "NoFail";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_nofail;
        public override string Description => "You can't fail, no matter what.";
        public override double ScoreMultiplier => 0.5;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModRelax), typeof(ModSuddenDeath), typeof(ModPerfect) };
    }

    public abstract class ModEasy : Mod
    {
        public override string Name => "Easy";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_easy;
        public override string Description => "Reduces overall difficulty - larger circles, more forgiving HP drain, less accuracy required.";
        public override double ScoreMultiplier => 0.5;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModHardRock) };
    }

    public abstract class ModHidden : Mod
    {
        public override string Name => "Hidden";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_hidden;
        public override bool Ranked => true;
    }

    public abstract class ModHardRock : Mod
    {
        public override string Name => "Hard Rock";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_hardrock;
        public override string Description => "Everything just got a bit harder...";
        public override Type[] IncompatibleMods => new[] { typeof(ModEasy) };
    }

    public abstract class ModSuddenDeath : Mod
    {
        public override string Name => "Sudden Death";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_suddendeath;
        public override string Description => "Miss a note and fail.";
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModNoFail), typeof(ModRelax), typeof(ModAutoplay), typeof(ModCinema) };
    }

    public abstract class ModDoubleTime : Mod
    {
        public override string Name => "Double Time";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_doubletime;
        public override string Description => "Zoooooooooom";
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModHalfTime) };
    }

    public abstract class ModRelax : Mod
    {
        public override string Name => "Relax";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_relax;
        public override double ScoreMultiplier => 0;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(ModCinema), typeof(ModNoFail), typeof(ModSuddenDeath), typeof(ModPerfect) };
    }

    public abstract class ModHalfTime : Mod
    {
        public override string Name => "Half Time";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_halftime;
        public override string Description => "Less zoom";
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModDoubleTime), typeof(ModNightcore) };
    }

    public abstract class ModNightcore : ModDoubleTime
    {
        public override string Name => "Nightcore";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_nightcore;
        public override string Description => "uguuuuuuuu";
    }

    public abstract class ModFlashlight : Mod
    {
        public override string Name => "Flashlight";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_flashlight;
        public override string Description => "Restricted view area.";
        public override bool Ranked => true;
    }

    public class ModAutoplay : Mod
    {
        public override string Name => "Autoplay";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_auto;
        public override string Description => "Watch a perfect automated play through the song";
        public override double ScoreMultiplier => 0;
        public override Type[] IncompatibleMods => new[] { typeof(ModRelax), typeof(ModSuddenDeath), typeof(ModPerfect) };

        public override void PlayerLoading(Player player)
        {
            base.PlayerLoading(player);
            player.ReplayInputHandler = Ruleset.GetRuleset(player.Beatmap.PlayMode).CreateAutoplayScore(player.Beatmap.Beatmap)?.Replay?.GetInputHandler();
        }
    }

    public abstract class ModPerfect : ModSuddenDeath
    {
        public override string Name => "Perfect";
        public override string Description => "SS or quit.";
    }

    public class ModCinema : ModAutoplay
    {
        public override string Name => "Cinema";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_cinema;
    }

    public enum ModType
    {
        DifficultyReduction,
        DifficultyIncrease,
        Special,
    }
}
