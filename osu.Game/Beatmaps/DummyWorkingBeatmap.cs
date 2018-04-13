// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Beatmaps
{
    public class DummyWorkingBeatmap : WorkingBeatmap
    {
        private readonly OsuGameBase game;

        public DummyWorkingBeatmap(OsuGameBase game)
            : base(new BeatmapInfo
            {
                Metadata = new BeatmapMetadata
                {
                    Artist = "please load a beatmap!",
                    Title = "no beatmaps available!"
                },
                BeatmapSet = new BeatmapSetInfo(),
                BaseDifficulty = new BeatmapDifficulty
                {
                    DrainRate = 0,
                    CircleSize = 0,
                    OverallDifficulty = 0,
                    ApproachRate = 0,
                    SliderMultiplier = 0,
                    SliderTickRate = 0,
                },
                Ruleset = new DummyRulesetInfo()
            })
        {
            this.game = game;
        }

        protected override Beatmap GetBeatmap() => new Beatmap();

        protected override Texture GetBackground() => game.Textures.Get(@"Backgrounds/bg4");

        protected override Track GetTrack() => new TrackVirtual();

        private class DummyRulesetInfo : RulesetInfo
        {
            public override Ruleset CreateInstance() => new DummyRuleset(this);

            private class DummyRuleset : Ruleset
            {
                public override IEnumerable<Mod> GetModsFor(ModType type) => new Mod[] { };

                public override RulesetContainer CreateRulesetContainerWith(WorkingBeatmap beatmap, bool isForCurrentRuleset)
                {
                    throw new NotImplementedException();
                }

                public override DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap, Mod[] mods = null) => null;

                public override string Description => "dummy";

                public override string ShortName => "dummy";

                public DummyRuleset(RulesetInfo rulesetInfo = null)
                    : base(rulesetInfo)
                {
                }
            }
        }
    }
}
