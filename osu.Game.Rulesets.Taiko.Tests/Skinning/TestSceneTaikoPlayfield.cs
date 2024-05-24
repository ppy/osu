// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;
using osuTK;

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

            AddStep("Load playfield", () => SetContents(_ => new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(2f, 1f),
                Scale = new Vector2(0.5f),
                Child = new TaikoPlayfieldAdjustmentContainer { Child = new TaikoPlayfield() },
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
            int value = 0;

            AddRepeatStep("change height", () =>
            {
                value = (value + 1) % 5;

                this.ChildrenOfType<TaikoPlayfieldAdjustmentContainer>().ForEach(p =>
                {
                    var parent = (Container)p.Parent.AsNonNull();
                    parent.Scale = new Vector2(0.5f + 0.1f * value);
                    parent.Width = 1f / parent.Scale.X;
                    parent.Height = 0.5f / parent.Scale.Y;
                });
            }, 50);
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
