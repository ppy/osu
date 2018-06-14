// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaDifficultyAdjustmentMods : DifficultyAdjustmentMods
    {
        protected override Mod[] Mods => new Mod[]
        {
            new ManiaModDoubleTime(),
            new ManiaModHalfTime(),
            new ManiaModEasy(),
            new ManiaModHardRock(),
            new ManiaModKey1(),
            new ManiaModKey2(),
            new ManiaModKey3(),
            new ManiaModKey4(),
            new ManiaModKey5(),
            new ManiaModKey6(),
            new ManiaModKey7(),
            new ManiaModKey8(),
            new ManiaModKey9(),
        };
    }
}
