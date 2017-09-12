// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Users;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModNoFail : ModNoFail
    {

    }

    public class ManiaModEasy : ModEasy
    {

    }

    public class ManiaModHidden : ModHidden
    {
        public override string Description => @"The notes fade out before you hit them!";
        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => new[] { typeof(ModFlashlight) };
    }

    public class ManiaModHardRock : ModHardRock
    {
        public override double ScoreMultiplier => 1.0;
    }

    public class ManiaModSuddenDeath : ModSuddenDeath
    {

    }

    public class ManiaModDaycore : ModDaycore
    {
        public override double ScoreMultiplier => 0.3;
    }

    public class ManiaModDoubleTime : ModDoubleTime
    {
        public override double ScoreMultiplier => 1.0;
    }

    public class ManiaModHalfTime : ModHalfTime
    {
        public override double ScoreMultiplier => 0.3;
    }

    public class ManiaModNightcore : ModNightcore
    {
        public override double ScoreMultiplier => 1.0;
    }

    public class ManiaModFlashlight : ModFlashlight
    {
        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => new[] { typeof(ModHidden) };
    }

    public class ManiaModPerfect : ModPerfect
    {

    }

    public class ManiaModFadeIn : Mod
    {
        public override string Name => "FadeIn";
        public override string ShortenedName => "FI";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_hidden;
        public override ModType Type => ModType.DifficultyIncrease;
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModFlashlight) };
    }

    public class ManiaModRandom : Mod
    {
        public override string Name => "Random";
        public override string ShortenedName => "RD";
        public override string Description => @"Shuffle around the notes!";
        public override double ScoreMultiplier => 1;
    }

    public abstract class ManiaKeyMod : Mod
    {
        public override string ShortenedName => Name;
        public abstract int KeyCount { get; }
        public override double ScoreMultiplier => 1; // TODO: Implement the mania key mod score multiplier
        public override bool Ranked => true;
    }

    public class ManiaModKey1 : ManiaKeyMod
    {
        public override int KeyCount => 1;
        public override string Name => "1K";
    }

    public class ManiaModKey2 : ManiaKeyMod
    {
        public override int KeyCount => 2;
        public override string Name => "2K";
    }

    public class ManiaModKey3 : ManiaKeyMod
    {
        public override int KeyCount => 3;
        public override string Name => "3K";
    }

    public class ManiaModKey4 : ManiaKeyMod
    {
        public override int KeyCount => 4;
        public override string Name => "4K";
    }

    public class ManiaModKey5 : ManiaKeyMod
    {
        public override int KeyCount => 5;
        public override string Name => "5K";
    }

    public class ManiaModKey6 : ManiaKeyMod
    {
        public override int KeyCount => 6;
        public override string Name => "6K";
    }

    public class ManiaModKey7 : ManiaKeyMod
    {
        public override int KeyCount => 7;
        public override string Name => "7K";
    }

    public class ManiaModKey8 : ManiaKeyMod
    {
        public override int KeyCount => 8;
        public override string Name => "8K";
    }

    public class ManiaModKey9 : ManiaKeyMod
    {
        public override int KeyCount => 9;
        public override string Name => "9K";
    }

    public class ManiaModKeyCoop : Mod
    {
        public override string Name => "KeyCoop";
        public override string ShortenedName => "2P";
        public override string Description => @"Double the key amount, double the fun!";
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;
    }

    public class ManiaModAutoplay : ModAutoplay<ManiaHitObject>
    {
        private int availableColumns;

        public override void ApplyToRulesetContainer(RulesetContainer<ManiaHitObject> rulesetContainer)
        {
            // Todo: This shouldn't be done, we should be getting a ManiaBeatmap which should store AvailableColumns
            // But this is dependent on a _lot_ of refactoring
            var maniaRulesetContainer = (ManiaRulesetContainer)rulesetContainer;
            availableColumns = maniaRulesetContainer.AvailableColumns;

            base.ApplyToRulesetContainer(rulesetContainer);
        }
        protected override Score CreateReplayScore(Beatmap<ManiaHitObject> beatmap) => new Score
        {
            User = new User { Username = "osu!topus!" },
            Replay = new ManiaAutoGenerator(beatmap, availableColumns).Generate(),
        };
    }
}
