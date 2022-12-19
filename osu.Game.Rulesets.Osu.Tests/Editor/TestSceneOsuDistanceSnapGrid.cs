// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Overlays;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public partial class TestSceneOsuDistanceSnapGrid : OsuManualInputManagerTestScene
    {
        private const float beat_length = 100;

        private static readonly Vector2 grid_position = new Vector2(512, 384);

        [Cached(typeof(EditorBeatmap))]
        [Cached(typeof(IBeatSnapProvider))]
        private readonly EditorBeatmap editorBeatmap;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [Cached]
        private readonly EditorClock editorClock;

        [Cached]
        private readonly BindableBeatDivisor beatDivisor = new BindableBeatDivisor();

        [Cached(typeof(IDistanceSnapProvider))]
        private readonly OsuHitObjectComposer snapProvider = new OsuHitObjectComposer(new OsuRuleset())
        {
            // Just used for the snap implementation, so let's hide from vision.
            AlwaysPresent = true,
            Alpha = 0,
        };

        private OsuDistanceSnapGrid grid;
        private SnappingCursorContainer cursor;

        public TestSceneOsuDistanceSnapGrid()
        {
            editorBeatmap = new EditorBeatmap(new OsuBeatmap
            {
                BeatmapInfo =
                {
                    Ruleset = new OsuRuleset().RulesetInfo
                }
            });

            base.Content.Children = new Drawable[]
            {
                editorClock = new EditorClock(editorBeatmap),
                snapProvider,
                Content
            };
        }

        protected override Container<Drawable> Content { get; } = new Container { RelativeSizeAxes = Axes.Both };

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            editorBeatmap.Difficulty.SliderMultiplier = 1;
            editorBeatmap.ControlPointInfo.Clear();
            editorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = beat_length });
            snapProvider.DistanceSpacingMultiplier.Value = 1;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.SlateGray
                },
                cursor = new SnappingCursorContainer { GetSnapPosition = v => grid.GetSnappedPosition(grid.ToLocalSpace(v)).position },
                grid = new OsuDistanceSnapGrid(new HitCircle { Position = grid_position }),
            };
        });

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(6)]
        [TestCase(8)]
        [TestCase(12)]
        [TestCase(16)]
        public void TestBeatDivisor(int divisor)
        {
            AddStep($"set beat divisor = {divisor}", () => beatDivisor.Value = divisor);
        }

        [TestCase(1.0f)]
        [TestCase(2.0f)]
        [TestCase(0.5f)]
        public void TestDistanceSpacing(float multiplier)
        {
            AddStep($"set distance spacing = {multiplier}", () => snapProvider.DistanceSpacingMultiplier.Value = multiplier);
        }

        [Test]
        public void TestCursorInCentre()
        {
            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(grid.ToScreenSpace(grid_position)));
            assertSnappedDistance(beat_length);
        }

        [Test]
        public void TestCursorAlmostInCentre()
        {
            AddStep("move mouse to almost centre", () => InputManager.MoveMouseTo(grid.ToScreenSpace(grid_position) + new Vector2(1)));
            assertSnappedDistance(beat_length);
        }

        [Test]
        public void TestCursorBeforeMovementPoint()
        {
            AddStep("move mouse to just before movement point", () => InputManager.MoveMouseTo(grid.ToScreenSpace(grid_position + new Vector2(beat_length, 0) * 1.45f)));
            assertSnappedDistance(beat_length);
        }

        [Test]
        public void TestCursorAfterMovementPoint()
        {
            AddStep("move mouse to just after movement point", () => InputManager.MoveMouseTo(grid.ToScreenSpace(grid_position + new Vector2(beat_length, 0) * 1.55f)));
            assertSnappedDistance(beat_length * 2);
        }

        [TestCase(0.5f, beat_length * 2)]
        [TestCase(1, beat_length * 2)]
        [TestCase(1.5f, beat_length * 1.5f)]
        [TestCase(2f, beat_length * 2)]
        public void TestDistanceSpacingAdjust(float multiplier, float expectedDistance)
        {
            AddStep($"Set distance spacing to {multiplier}", () => snapProvider.DistanceSpacingMultiplier.Value = multiplier);
            AddStep("move mouse to point", () => InputManager.MoveMouseTo(grid.ToScreenSpace(grid_position + new Vector2(beat_length, 0) * 2)));

            assertSnappedDistance(expectedDistance);
        }

        [Test]
        public void TestReferenceObjectNotOnSnapGrid()
        {
            AddStep("create grid", () =>
            {
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.SlateGray
                    },
                    cursor = new SnappingCursorContainer { GetSnapPosition = v => grid.GetSnappedPosition(grid.ToLocalSpace(v)).position },
                    grid = new OsuDistanceSnapGrid(new HitCircle
                    {
                        Position = grid_position,
                        // This is important. It sets the reference object to a point in time that isn't on the current snap divisor's grid.
                        // We are testing that the grid's display is offset correctly.
                        StartTime = 40,
                    }),
                };
            });

            AddStep("move mouse to point", () => InputManager.MoveMouseTo(grid.ToScreenSpace(grid_position + new Vector2(beat_length, 0) * 2)));

            AddAssert("Ensure cursor is on a grid line", () =>
            {
                return grid.ChildrenOfType<CircularProgress>().Any(p => Precision.AlmostEquals(p.ScreenSpaceDrawQuad.TopRight.X, grid.ToScreenSpace(cursor.LastSnappedPosition).X));
            });
        }

        [Test]
        public void TestLimitedDistance()
        {
            AddStep("create limited grid", () =>
            {
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.SlateGray
                    },
                    cursor = new SnappingCursorContainer { GetSnapPosition = v => grid.GetSnappedPosition(grid.ToLocalSpace(v)).position },
                    grid = new OsuDistanceSnapGrid(new HitCircle { Position = grid_position }, new HitCircle { StartTime = 200 }),
                };
            });

            AddStep("move mouse outside grid", () => InputManager.MoveMouseTo(grid.ToScreenSpace(grid_position + new Vector2(beat_length, 0) * 3f)));
            assertSnappedDistance(beat_length);
        }

        private void assertSnappedDistance(float expectedDistance) => AddAssert($"snap distance = {expectedDistance}", () =>
        {
            Vector2 snappedPosition = grid.GetSnappedPosition(grid.ToLocalSpace(InputManager.CurrentState.Mouse.Position)).position;

            return Precision.AlmostEquals(expectedDistance, Vector2.Distance(snappedPosition, grid_position));
        });

        private partial class SnappingCursorContainer : CompositeDrawable
        {
            public Func<Vector2, Vector2> GetSnapPosition;

            public Vector2 LastSnappedPosition { get; private set; }

            private readonly Drawable cursor;

            private InputManager inputManager;

            public override bool HandlePositionalInput => true;

            public SnappingCursorContainer()
            {
                RelativeSizeAxes = Axes.Both;

                InternalChild = cursor = new Circle
                {
                    Origin = Anchor.Centre,
                    Size = new Vector2(50),
                    Colour = Color4.Red
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                inputManager = GetContainingInputManager();
            }

            protected override void Update()
            {
                base.Update();
                cursor.Position = LastSnappedPosition = GetSnapPosition.Invoke(inputManager.CurrentState.Mouse.Position);
            }
        }
    }
}
