// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests
{
    public class TestSceneDrawableManiaHitObject : OsuTestScene
    {
        private readonly ManualClock clock = new ManualClock();

        private Column column;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = new ScrollingTestContainer(ScrollingDirection.Down)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
                TimeRange = 2000,
                Clock = new FramedClock(clock),
                Child = column = new Column(0)
                {
                    Action = { Value = ManiaAction.Key1 },
                    Height = 0.85f,
                    AccentColour = Color4.Gray
                },
            };
        });

        [Test]
        public void TestHoldNoteHeadVisibility()
        {
            DrawableHoldNote note = null;
            AddStep("Add hold note", () =>
            {
                var h = new HoldNote
                {
                    StartTime = 0,
                    Duration = 1000
                };
                h.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
                column.Add(note = new DrawableHoldNote(h));
            });
            AddStep("Hold key", () =>
            {
                clock.CurrentTime = 0;
                note.OnPressed(ManiaAction.Key1);
            });
            AddStep("progress time", () => clock.CurrentTime = 500);
            AddAssert("head is visible", () => note.Head.Alpha == 1);
        }
    }
}
