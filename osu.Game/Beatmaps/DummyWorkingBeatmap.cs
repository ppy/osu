// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Beatmaps
{
    public class DummyWorkingBeatmap : WorkingBeatmap
    {
        private readonly OsuGameBase game;

        public DummyWorkingBeatmap(OsuGameBase game = null)
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
                },
                Ruleset = new DummyRulesetInfo()
            })
        {
            this.game = game;
        }

        protected override IBeatmap GetBeatmap() => new Beatmap();

        protected override Texture GetBackground() => game?.Textures.Get(@"Backgrounds/bg4");

        protected override Track GetTrack() => new TrackVirtual { Length = 1000 };

        private class DummyRulesetInfo : RulesetInfo
        {
            public override Ruleset CreateInstance() => new DummyRuleset(this);

            private class DummyRuleset : Ruleset
            {
                public override IEnumerable<Mod> GetModsFor(ModType type) => new Mod[] { };

                public override DrawableRuleset CreateDrawableRulesetWith(WorkingBeatmap beatmap)
                {
                    throw new NotImplementedException();
                }

                public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new DummyBeatmapConverter { Beatmap = beatmap };

                public override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap) => null;

                public override string Description => "dummy";

                public override string ShortName => "dummy";

                public DummyRuleset(RulesetInfo rulesetInfo = null)
                    : base(rulesetInfo)
                {
                }

                private class DummyBeatmapConverter : IBeatmapConverter
                {
                    public event Action<HitObject, IEnumerable<HitObject>> ObjectConverted;
                    public IBeatmap Beatmap { get; set; }
                    public bool CanConvert => true;
                    public IBeatmap Convert() => Beatmap;
                }
            }
        }
    }
}
