// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using JetBrains.Annotations;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Beatmaps
{
    public class DummyWorkingBeatmap : WorkingBeatmap
    {
        private readonly TextureStore textures;

        public DummyWorkingBeatmap([NotNull] AudioManager audio, TextureStore textures)
            : base(new BeatmapInfo
            {
                Metadata = new BeatmapMetadata
                {
                    Artist = "please load a beatmap!",
                    Title = "no beatmaps available!"
                },
                BeatmapSet = new BeatmapSetInfo(),
                Difficulty = new BeatmapDifficulty
                {
                    CircleSize = 0,
                    DrainRate = 0,
                    OverallDifficulty = 0,
                    ApproachRate = 0,
                },
                Ruleset = new DummyRuleset().RulesetInfo
            }, audio)
        {
            this.textures = textures;

            // We are guaranteed to have a virtual track.
            // To ease usability, ensure the track is available from point of construction.
            LoadTrack();
        }

        protected override IBeatmap GetBeatmap() => new Beatmap();

        public override Texture GetBackground() => textures?.Get(@"Backgrounds/bg2");

        protected override Track GetBeatmapTrack() => GetVirtualTrack();

        protected internal override ISkin GetSkin() => null;

        public override Stream GetStream(string storagePath) => null;

        private class DummyRuleset : Ruleset
        {
            public override IEnumerable<Mod> GetModsFor(ModType type) => Array.Empty<Mod>();

            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
            {
                throw new NotImplementedException();
            }

            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new DummyBeatmapConverter(beatmap);

            public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => throw new NotImplementedException();

            public override string Description => "dummy";

            public override string ShortName => "dummy";

            private class DummyBeatmapConverter : IBeatmapConverter
            {
                public IBeatmap Beatmap { get; }

                public DummyBeatmapConverter(IBeatmap beatmap)
                {
                    Beatmap = beatmap;
                }

                [CanBeNull]
                public event Action<HitObject, IEnumerable<HitObject>> ObjectConverted;

                public bool CanConvert() => true;

                public IBeatmap Convert(CancellationToken cancellationToken = default)
                {
                    foreach (var obj in Beatmap.HitObjects)
                        ObjectConverted?.Invoke(obj, obj.Yield());

                    return Beatmap;
                }
            }
        }
    }
}
