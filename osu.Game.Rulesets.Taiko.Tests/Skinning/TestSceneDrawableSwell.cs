// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    [TestFixture]
    public partial class TestSceneDrawableSwell : TaikoSkinnableTestScene
    {
        [Test]
        public void TestHits()
        {
            AddStep("Centre hit", () => SetContents(_ => new DrawableSwell(createHitAtCurrentTime())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }));
        }

        private Swell createHitAtCurrentTime()
        {
            var hit = new Swell
            {
                StartTime = Time.Current + 3000,
                EndTime = Time.Current + 6000,
            };

            hit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            return hit;
        }
    }
}
