// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using osu.Game.Tests.Visual;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestCaseSlider : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(DrawableSlider) };

        private readonly Container content;
        protected override Container<Drawable> Content => content;

        public TestCaseSlider()
        {
            base.Content.Add(content = new OsuInputManager(new RulesetInfo { ID = 0 }));

            AddStep("Single", addSingle);
            AddStep("Repeated (1)", () => addRepeated(1));
            AddStep("Repeated (2)", () => addRepeated(2));
            AddStep("Repeated (3)", () => addRepeated(3));
            AddStep("Repeated (4)", () => addRepeated(4));
            AddStep("Stream", addStream);
        }

        private void addSingle()
        {
            var slider = new Slider
            {
                StartTime = Time.Current + 1000,
                Position = new Vector2(-200, 0),
                ControlPoints = new List<Vector2>
                {
                    new Vector2(-200, 0),
                    new Vector2(400, 0),
                },
                Distance = 400,
                Velocity = 1,
                TickDistance = 100,
            };

            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            Add(new DrawableSlider(slider) { Anchor = Anchor.Centre });
        }

        private void addRepeated(int repeats)
        {
            var slider = new Slider
            {
                StartTime = Time.Current + 1000,
                Position = new Vector2(-200, 0),
                ControlPoints = new List<Vector2>
                {
                    new Vector2(-200, 0),
                    new Vector2(400, 0),
                },
                Distance = 400,
                Velocity = 11,
                TickDistance = 100,
                RepeatCount = repeats,
                RepeatSamples = new List<SampleInfo>[repeats].ToList()
            };

            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            Add(new DrawableSlider(slider) { Anchor = Anchor.Centre });
        }

        private void addStream()
        {

        }
    }
}
