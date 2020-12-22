// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public abstract class HitObjectApplicationTestScene : OsuTestScene
    {
        [Cached(typeof(IScrollingInfo))]
        private ScrollingTestContainer.TestScrollingInfo info = new ScrollingTestContainer.TestScrollingInfo
        {
            Direction = { Value = ScrollingDirection.Left },
            TimeRange = { Value = 1000 },
        };

        private ScrollingHitObjectContainer hitObjectContainer;

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = hitObjectContainer = new ScrollingHitObjectContainer
            {
                RelativeSizeAxes = Axes.X,
                Height = 200,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Clock = new FramedClock(new StopwatchClock())
            };
        }

        [SetUpSteps]
        public void SetUp()
            => AddStep("clear SHOC", () => hitObjectContainer.Clear(false));

        protected void AddHitObject(DrawableHitObject hitObject)
            => AddStep("add to SHOC", () => hitObjectContainer.Add(hitObject));

        protected void RemoveHitObject(DrawableHitObject hitObject)
            => AddStep("remove from SHOC", () => hitObjectContainer.Remove(hitObject));

        protected TObject PrepareObject<TObject>(TObject hitObject)
            where TObject : TaikoHitObject
        {
            hitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
            return hitObject;
        }
    }
}
