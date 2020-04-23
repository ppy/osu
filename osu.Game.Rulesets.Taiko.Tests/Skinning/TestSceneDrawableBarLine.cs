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
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    [TestFixture]
    public class TestSceneDrawableBarLine : TaikoSkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => base.RequiredTypes.Concat(new[]
        {
            typeof(DrawableBarLine),
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
            AddStep("Bar line ", () => SetContents(() =>
            {
                var hoc = new ScrollingHitObjectContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Height = 0.8f
                };

                hoc.Add(new DrawableBarLine(createBarLineAtCurrentTime())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });

                return hoc;
            }));

            AddStep("Bar line (major)", () => SetContents(() =>
            {
                var hoc = new ScrollingHitObjectContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Height = 0.8f
                };

                hoc.Add(new DrawableBarLineMajor(createBarLineAtCurrentTime(true))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });

                return hoc;
            }));
        }

        private BarLine createBarLineAtCurrentTime(bool major = false)
        {
            var drumroll = new BarLine
            {
                Major = major,
                StartTime = Time.Current + 1000,
            };

            var cpi = new ControlPointInfo();
            cpi.Add(0, new TimingControlPoint { BeatLength = 500 });

            drumroll.ApplyDefaults(cpi, new BeatmapDifficulty());

            return drumroll;
        }
    }
}
