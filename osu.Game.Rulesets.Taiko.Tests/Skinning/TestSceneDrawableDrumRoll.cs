// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Skinning;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    [TestFixture]
    public class TestSceneDrawableDrumRoll : TaikoSkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => base.RequiredTypes.Concat(new[]
        {
            typeof(DrawableDrumRoll),
            typeof(DrawableDrumRollTick),
            typeof(LegacyDrumRoll),
        }).ToList();

        [Cached(typeof(IScrollingInfo))]
        private ScrollingTestContainer.TestScrollingInfo info = new ScrollingTestContainer.TestScrollingInfo
        {
            Direction = { Value = ScrollingDirection.Left },
            TimeRange = { Value = 5000 },
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            AddStep("Drum roll", () => SetContents(() =>
            {
                var hoc = new ScrollingHitObjectContainer();

                hoc.Add(new DrawableDrumRoll(createDrumRollAtCurrentTime())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 500,
                });

                return hoc;
            }));

            AddStep("Drum roll (strong)", () => SetContents(() =>
            {
                var hoc = new ScrollingHitObjectContainer();

                hoc.Add(new DrawableDrumRoll(createDrumRollAtCurrentTime(true))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 500,
                });

                return hoc;
            }));
        }

        private DrumRoll createDrumRollAtCurrentTime(bool strong = false)
        {
            var drumroll = new DrumRoll
            {
                IsStrong = strong,
                StartTime = Time.Current + 1000,
                Duration = 4000,
            };

            var cpi = new ControlPointInfo();
            cpi.Add(0, new TimingControlPoint { BeatLength = 500 });

            drumroll.ApplyDefaults(cpi, new BeatmapDifficulty());

            return drumroll;
        }
    }
}
