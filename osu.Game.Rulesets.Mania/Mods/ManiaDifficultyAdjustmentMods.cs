// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaDifficultyAdjustmentMods : DifficultyAdjustmentMods
    {
        private readonly ManiaBeatmap beatmap;

        public ManiaDifficultyAdjustmentMods(IBeatmap beatmap)
        {
            this.beatmap = beatmap as ManiaBeatmap ?? throw new ArgumentException($"Expected provided beatmap to be a {typeof(ManiaBeatmap)}.", nameof(beatmap));
        }

        protected override Mod[] Mods
        {
            get
            {
                if (beatmap.IsManiaBeatmap)
                {
                    return new Mod[]
                    {
                        new ManiaModDoubleTime(),
                        new ManiaModHalfTime(),
                        new ManiaModEasy(),
                        new ManiaModHardRock(),
                    };
                }

                return new Mod[]
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

    }
}
