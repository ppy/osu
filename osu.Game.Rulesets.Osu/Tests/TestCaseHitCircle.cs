// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    [Ignore("getting CI working")]
    public class TestCaseHitCircle : OsuTestCase
    {
        private readonly Container content;
        protected override Container<Drawable> Content => content;

        private bool auto;
        private int depthIndex;

        public TestCaseHitCircle()
        {
            base.Content.Add(content = new OsuInputManager(new RulesetInfo { ID = 0 }));

            AddStep("Single", () => addSingle());
            AddStep("Stream", addStream);
            AddToggleStep("Auto", v => auto = v);
        }

        private void addSingle(double timeOffset = 0, Vector2? positionOffset = null)
        {
            positionOffset = positionOffset ?? Vector2.Zero;

            var circle = new HitCircle
            {
                StartTime = Time.Current + 1000 + timeOffset,
                Position = positionOffset.Value
            };

            circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 0 });

            var drawable = new DrawableHitCircle(circle)
            {
                Anchor = Anchor.Centre,
                Depth = depthIndex++
            };

            if (auto)
                drawable.State.Value = ArmedState.Hit;

            Add(drawable);
        }

        private void addStream()
        {
            Vector2 pos = Vector2.Zero;

            for (int i = 0; i <= 1000; i += 100)
            {
                addSingle(i, pos);
                pos += new Vector2(10);
            }
        }
    }
}
