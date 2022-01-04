// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    [TestFixture]
    public class TestSceneDrawableBarLine : TaikoSkinnableTestScene
    {
        [Cached(typeof(IScrollingInfo))]
        private ScrollingTestContainer.TestScrollingInfo info = new ScrollingTestContainer.TestScrollingInfo
        {
            Direction = { Value = ScrollingDirection.Left },
            TimeRange = { Value = 5000 },
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            AddStep("Bar line", () => SetContents(_ =>
            {
                ScrollingHitObjectContainer hoc;

                var cont = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.8f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new TaikoPlayfield(),
                        hoc = new ScrollingHitObjectContainer()
                    }
                };

                hoc.Add(new DrawableBarLine(createBarLineAtCurrentTime())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });

                return cont;
            }));

            AddStep("Bar line (major)", () => SetContents(_ =>
            {
                ScrollingHitObjectContainer hoc;

                var cont = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.8f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new TaikoPlayfield(),
                        hoc = new ScrollingHitObjectContainer()
                    }
                };

                hoc.Add(new DrawableBarLine(createBarLineAtCurrentTime(true))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });

                return cont;
            }));
        }

        private BarLine createBarLineAtCurrentTime(bool major = false)
        {
            var barLine = new BarLine
            {
                Major = major,
                StartTime = Time.Current + 2000,
            };

            var cpi = new ControlPointInfo();
            cpi.Add(0, new TimingControlPoint { BeatLength = 500 });

            barLine.ApplyDefaults(cpi, new BeatmapDifficulty());

            return barLine;
        }
    }
}
