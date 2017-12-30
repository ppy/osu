// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    [Ignore("getting CI working")]
    public class TestCaseSpinner : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
{
            typeof(Spinner),
            typeof(OsuModHidden),
            typeof(DrawableSpinner)
        };

        private readonly Container content;
        protected override Container<Drawable> Content => content;

        private bool hidden;
        private int depthIndex;
        private int circleSize;
        private float circleScale = 1;

        public TestCaseSpinner()
        {
            base.Content.Add(content = new OsuInputManager(new RulesetInfo { ID = 0 }));

            AddStep("Single", testSingle);
            AddToggleStep("Hidden", v => hidden = v);
            AddSliderStep("CircleSize", 0, 10, 0, s => circleSize = s);
            AddSliderStep("CircleScale", 0.5f, 2, 1, s => circleScale = s);
        }

        private void testSingle()
        {
            var spinner = new Spinner { StartTime = Time.Current + 1000, EndTime = Time.Current + 4000 };

            spinner.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = circleSize });

            var drawable = new DrawableSpinner(spinner)
            {
                Anchor = Anchor.Centre,
                Scale = new Vector2(circleScale),
                Depth = depthIndex++
            };

            if (hidden)
                new OsuModHidden().ApplyToDrawableHitObjects(new [] { drawable });

            Add(drawable);
        }
    }
}
