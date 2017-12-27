// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestCaseHitCircle : OsuTestCase
    {
        private readonly Container content;
        protected override Container<Drawable> Content => content;

        public TestCaseHitCircle()
        {
            base.Content.Add(content = new OsuInputManager(new RulesetInfo { ID = 0 }));

            AddStep("Single", addHitCircle);
            AddStep("Stream", addStream);
        }

        private void addHitCircle()
        {
            var circle = new HitCircle { StartTime = Time.Current + 1000 };
            circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            Add(new DrawableHitCircle(circle) { Anchor = Anchor.Centre });
        }

        private void addStream()
        {
            Vector2 pos = Vector2.Zero;

            for (int i = 1000; i <= 2000; i += 100)
            {
                var circle = new HitCircle { StartTime = Time.Current + i, Position = pos };
                circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                Add(new DrawableHitCircle(circle) { Anchor = Anchor.Centre, Depth = i});

                pos += new Vector2(10);
            }
        }
    }
}
