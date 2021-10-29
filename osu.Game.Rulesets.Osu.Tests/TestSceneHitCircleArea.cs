// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneHitCircleArea : OsuManualInputManagerTestScene
    {
        private HitCircle hitCircle;
        private DrawableHitCircle drawableHitCircle;
        private DrawableHitCircle.HitReceptor hitAreaReceptor => drawableHitCircle.HitArea;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            hitCircle = new HitCircle
            {
                Position = new Vector2(100, 100),
                StartTime = Time.Current + 500
            };

            hitCircle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            Child = new SkinProvidingContainer(new DefaultSkin(null))
            {
                RelativeSizeAxes = Axes.Both,
                Child = drawableHitCircle = new DrawableHitCircle(hitCircle)
                {
                    Size = new Vector2(100)
                }
            };
        });

        [Test]
        public void TestCircleHitCentre()
        {
            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(hitAreaReceptor.ScreenSpaceDrawQuad.Centre));
            scheduleHit();

            AddAssert("hit registered", () => hitAreaReceptor.HitAction == OsuAction.LeftButton);
        }

        [Test]
        public void TestCircleHitLeftEdge()
        {
            AddStep("move mouse to left edge", () =>
            {
                var drawQuad = hitAreaReceptor.ScreenSpaceDrawQuad;
                var mousePosition = new Vector2(drawQuad.TopLeft.X, drawQuad.Centre.Y);

                InputManager.MoveMouseTo(mousePosition);
            });
            scheduleHit();

            AddAssert("hit registered", () => hitAreaReceptor.HitAction == OsuAction.LeftButton);
        }

        [TestCase(0.95f, OsuAction.LeftButton)]
        [TestCase(1.05f, null)]
        public void TestHitsCloseToEdge(float relativeDistanceFromCentre, OsuAction? expectedAction)
        {
            AddStep("move mouse to top left circle edge", () =>
            {
                var drawQuad = hitAreaReceptor.ScreenSpaceDrawQuad;
                // sqrt(2) / 2 = sin(45deg) = cos(45deg)
                // draw width halved to get radius
                float correction = relativeDistanceFromCentre * (float)Math.Sqrt(2) / 2 * (drawQuad.Width / 2);
                var mousePosition = new Vector2(drawQuad.Centre.X - correction, drawQuad.Centre.Y - correction);

                InputManager.MoveMouseTo(mousePosition);
            });
            scheduleHit();

            AddAssert($"hit {(expectedAction == null ? "not " : string.Empty)}registered", () => hitAreaReceptor.HitAction == expectedAction);
        }

        [Test]
        public void TestCircleMissBoundingBoxCorner()
        {
            AddStep("move mouse to top left corner of bounding box", () => InputManager.MoveMouseTo(hitAreaReceptor.ScreenSpaceDrawQuad.TopLeft));
            scheduleHit();

            AddAssert("hit not registered", () => hitAreaReceptor.HitAction == null);
        }

        private void scheduleHit() => AddStep("schedule action", () =>
        {
            double delay = hitCircle.StartTime - hitCircle.HitWindows.WindowFor(HitResult.Great) - Time.Current;
            Scheduler.AddDelayed(() => hitAreaReceptor.OnPressed(new KeyBindingPressEvent<OsuAction>(GetContainingInputManager().CurrentState, OsuAction.LeftButton)), delay);
        });
    }
}
