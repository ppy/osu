// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    public partial class TestSceneTaikoPlayfield : TaikoSkinnableTestScene
    {
        [Cached(typeof(IScrollingInfo))]
        private ScrollingTestContainer.TestScrollingInfo info = new ScrollingTestContainer.TestScrollingInfo
        {
            Direction = { Value = ScrollingDirection.Left },
            TimeRange = { Value = 5000 },
        };

        [SetUpSteps]
        public void SetUpSteps()
        {
            TaikoBeatmap beatmap;
            AddStep("set beatmap", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(beatmap = new TaikoBeatmap());

                beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 1000 });

                Beatmap.Value.Track.Start();
            });

            AddStep("Load playfield", () => SetContents(_ => new TaikoPlayfield
            {
                Height = 0.2f,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
            }));
        }

        [Test]
        public void TestBasic()
        {
            AddStep("do nothing", () => { });
        }

        [Test]
        public void TestHeightChanges()
        {
            AddRepeatStep("change height", () => this.ChildrenOfType<TaikoPlayfield>().ForEach(p => p.Height = Math.Max(0.2f, (p.Height + 0.2f) % 1f)), 50);
        }

        [Test]
        public void TestKiai()
        {
            bool kiai = false;

            AddStep("Toggle kiai", () =>
            {
                Beatmap.Value.Beatmap.ControlPointInfo.Add(0, new EffectControlPoint { KiaiMode = (kiai = !kiai) });
            });
        }
    }
}
