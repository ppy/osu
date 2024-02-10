// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Edit;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests.Editor
{
    public partial class TestSceneTaikoHitObjectComposer : EditorClockTestScene
    {
        [SetUp]
        public void Setup() => Schedule(() =>
        {
            BeatDivisor.Value = 8;
            EditorClock.Seek(0);

            Child = new TestComposer { RelativeSizeAxes = Axes.Both };
        });

        [Test]
        public void BasicTest()
        {
        }

        private partial class TestComposer : CompositeDrawable
        {
            [Cached(typeof(EditorBeatmap))]
            [Cached(typeof(IBeatSnapProvider))]
            public readonly EditorBeatmap EditorBeatmap;

            public TestComposer()
            {
                InternalChildren = new Drawable[]
                {
                    EditorBeatmap = new EditorBeatmap(new TaikoBeatmap
                    {
                        BeatmapInfo = { Ruleset = new TaikoRuleset().RulesetInfo }
                    }),
                    new TaikoHitObjectComposer(new TaikoRuleset())
                };

                for (int i = 0; i < 10; i++)
                    EditorBeatmap.Add(new Hit { StartTime = 125 * i });
            }
        }
    }
}
