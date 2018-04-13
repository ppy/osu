﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestCaseSpinner : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
{
            typeof(SpinnerDisc),
            typeof(DrawableSpinner),
            typeof(DrawableOsuHitObject)
        };

        private readonly Container content;
        protected override Container<Drawable> Content => content;

        private int depthIndex;
        protected readonly List<Mod> Mods = new List<Mod>();

        public TestCaseSpinner()
        {
            base.Content.Add(content = new OsuInputManager(new RulesetInfo { ID = 0 }));

            AddStep("Miss Big", () => testSingle(2));
            AddStep("Miss Medium", () => testSingle(5));
            AddStep("Miss Small", () => testSingle(7));
            AddStep("Hit Big", () => testSingle(2, true));
            AddStep("Hit Medium", () => testSingle(5, true));
            AddStep("Hit Small", () => testSingle(7, true));
        }

        private void testSingle(float circleSize, bool auto = false)
        {
            var spinner = new Spinner { StartTime = Time.Current + 1000, EndTime = Time.Current + 4000 };

            spinner.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = circleSize });

            var drawable = new TestDrawableSpinner(spinner, auto)
            {
                Anchor = Anchor.Centre,
                Depth = depthIndex++
            };

            foreach (var mod in Mods.OfType<IApplicableToDrawableHitObjects>())
                mod.ApplyToDrawableHitObjects(new[] { drawable });

            Add(drawable);
        }

        private class TestDrawableSpinner : DrawableSpinner
        {
            private bool auto;

            public TestDrawableSpinner(Spinner s, bool auto) : base(s)
            {
                this.auto = auto;
            }

            protected override void CheckForJudgements(bool userTriggered, double timeOffset)
            {
                if (auto && !userTriggered && Time.Current > Spinner.StartTime + Spinner.Duration / 2 && Progress < 1)
                {
                    // force completion only once to not break human interaction
                    Disc.RotationAbsolute = Spinner.SpinsRequired * 360;
                    auto = false;
                }

                base.CheckForJudgements(userTriggered, timeOffset);
            }
        }
    }
}
