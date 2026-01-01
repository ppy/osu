// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using NUnit.Framework;
using osu.Framework.Testing;
using osuTK.Graphics;
using osu.Game.Skinning.Components;
using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using System.Threading;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Skinning;
using System.Collections.Generic;

namespace osu.Game.Tests.Visual.Components
{
    public partial class TestSceneSkillsBreakdown : OsuTestScene
    {
        private TestBeatmapDifficultyCache difficultyCache = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.CacheAs<BeatmapDifficultyCache>(difficultyCache = new TestBeatmapDifficultyCache());

            AddRange(new Drawable[]
            {
                difficultyCache,
                new SkillsBreakdown
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new osuTK.Vector2(250)
                }
            });
        }

        [Test]
        public void TestEqualValues()
        {
            AddStep("1", () =>
            {
                difficultyCache.UseRandomAttributes = false;
                triggerCacheRecalc();
            });
        }

        [Test]
        public void TestRandomValues()
        {
            AddStep("2", () =>
            {
                difficultyCache.UseRandomAttributes = true;
                triggerCacheRecalc();
            });
        }

        private void triggerCacheRecalc()
        {
            // Invalidate cache to force recalc
            difficultyCache.Invalidate(Beatmap.Value.BeatmapInfo);

            // Change mods to trigger ValueChanged
            SelectedMods.Value = SelectedMods.Value.Count >= 0 ? new List<Mod>() : SelectedMods.Value.Append(new OsuModNoFail()).ToList();
        }

        private partial class TestBeatmapDifficultyCache : BeatmapDifficultyCache
        {
            public bool UseRandomAttributes;
            private Random random = new Random();

            protected override Task<StarDifficulty?> ComputeValueAsync(DifficultyCacheLookup lookup, CancellationToken token = default)
            {
                var attributes = new TestDifficultyAttributes(
                    Enumerable.Range(0, 4)
                        .Select(_ => UseRandomAttributes ? random.NextDouble() : 1.0)
                        .ToArray());

                return Task.FromResult<StarDifficulty?>(new StarDifficulty(attributes));
            }
        }

        private partial class TestDifficultyAttributes : DifficultyAttributes
        {
            private SkillValue[] values;

            public TestDifficultyAttributes(double[] values)
            {
                this.values = new SkillValue[values.Length];

                for (int i = 0; i < values.Length; i++)
                {
                    this.values[i] = new SkillValue { Value = values[i], SkillName = $"Test Skill {i + 1}" };
                }
            }
            public override SkillValue[] GetSkillValues() => values;
        }
    }
}
