// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Edit.Blueprints;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Mania.Tests.Editor
{
    public partial class TestSceneNotePlacementBlueprint : ManiaPlacementBlueprintTestScene
    {
        [SetUp]
        public void Setup() => Schedule(() =>
        {
            this.ChildrenOfType<HitObjectContainer>().ForEach(c => c.Clear());

            ResetPlacement();

            ((ScrollingTestContainer)HitObjectContainer).Direction = ScrollingDirection.Down;
        });

        [Test]
        public void TestPlaceBeforeCurrentTimeDownwards()
        {
            AddStep("seek", () =>
            {
                EditorClock.Seek(200);
            });
            AddStep("move mouse before current time", () =>
            {
                var column = this.ChildrenOfType<Column>().Single();
                InputManager.MoveMouseTo(column.ScreenSpacePositionAtTime(100));
            });

            AddStep("click", () => InputManager.Click(MouseButton.Left));

            AddAssert("note start time < 0", () => getNote().StartTime < 200);
        }

        [Test]
        public void TestPlaceAfterCurrentTimeDownwards()
        {
            AddStep("seek", () =>
            {
                EditorClock.Seek(200);
            });
            AddStep("move mouse after current time", () =>
            {
                var column = this.ChildrenOfType<Column>().Single();
                InputManager.MoveMouseTo(column.ScreenSpacePositionAtTime(300));
            });

            AddStep("click", () => InputManager.Click(MouseButton.Left));

            AddAssert("note start time > 0", () => getNote().StartTime > 200);
        }

        protected override IClock ColumnClock { get; } = new ManualClock { CurrentTime = 200 };

        private Note getNote() => this.ChildrenOfType<DrawableNote>().FirstOrDefault()?.HitObject;

        protected override DrawableHitObject CreateHitObject(HitObject hitObject) => new DrawableNote((Note)hitObject);
        protected override PlacementBlueprint CreateBlueprint() => new NotePlacementBlueprint();
    }
}
