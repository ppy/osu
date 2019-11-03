// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Video;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Beatmaps
{
    public class DummyWorkingBeatmap : WorkingBeatmap
    {
        private readonly TextureStore textures;

        public DummyWorkingBeatmap(AudioManager audio, TextureStore textures)
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
            }, audio)
        {
            this.textures = textures;
        }

        protected override IBeatmap GetBeatmap() => new Beatmap();

        protected override Texture GetBackground() => textures?.Get(@"Backgrounds/bg4");

        protected override VideoSprite GetVideo() => null;

        protected override Track GetTrack() => GetVirtualTrack();

        private class DummyRulesetInfo : RulesetInfo
        {
            public override Ruleset CreateInstance() => new DummyRuleset(this);

            private class DummyRuleset : Ruleset
            {
                public override IEnumerable<Mod> GetModsFor(ModType type) => new Mod[] { };

                public override DrawableRuleset CreateDrawableRulesetWith(IWorkingBeatmap beatmap, IReadOnlyList<Mod> mods)
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

                    public IBeatmap Convert()
                    {
                        foreach (var obj in Beatmap.HitObjects)
                            ObjectConverted?.Invoke(obj, obj.Yield());

                        return Beatmap;
                    }
                }
            }
        }
    }
}
