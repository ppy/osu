// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    [TestFixture]
    public class TestSceneDrawableHit : TaikoSkinnableTestScene
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            AddStep("Centre hit", () => SetContents(() => new DrawableHit(createHitAtCurrentTime())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }));

            AddStep("Centre hit (strong)", () => SetContents(() => new DrawableHit(createHitAtCurrentTime(true))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }));

            AddStep("Rim hit", () => SetContents(() => new DrawableHit(createHitAtCurrentTime())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }));

            AddStep("Rim hit (strong)", () => SetContents(() => new DrawableHit(createHitAtCurrentTime(true))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }));
        }

        private Hit createHitAtCurrentTime(bool strong = false)
        {
            var hit = new Hit
            {
                IsStrong = strong,
                StartTime = Time.Current + 3000,
            };

            hit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            return hit;
        }
    }
}
