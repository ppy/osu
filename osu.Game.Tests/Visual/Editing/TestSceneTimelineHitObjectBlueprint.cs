// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osuTK;
using osuTK.Input;
using static osu.Game.Screens.Edit.Compose.Components.Timeline.TimelineHitObjectBlueprint;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneTimelineHitObjectBlueprint : TimelineTestScene
    {
        public override Drawable CreateTestComponent() => new TimelineBlueprintContainer(Composer);

        [Test]
        public void TestContextMenu()
        {
            TimelineHitObjectBlueprint blueprint;

            AddStep("add object", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.Add(new HitCircle { StartTime = 3000 });
            });

            AddStep("click object", () =>
            {
                blueprint = this.ChildrenOfType<TimelineHitObjectBlueprint>().Single();
                InputManager.MoveMouseTo(blueprint);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("right click", () => InputManager.Click(MouseButton.Right));
            AddAssert("context menu open", () => this.ChildrenOfType<OsuContextMenu>().SingleOrDefault()?.State == MenuState.Open);

            AddStep("click object", () =>
            {
                blueprint = this.ChildrenOfType<TimelineHitObjectBlueprint>().Single();
                InputManager.MoveMouseTo(blueprint);
                InputManager.Click(MouseButton.Left);
            });
        }

        [Test]
        public void TestDisallowZeroDurationObjects()
        {
            DragArea dragArea;

            AddStep("add spinner", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.Add(new Spinner
                {
                    Position = new Vector2(256, 256),
                    StartTime = 2700,
                    Duration = 500
                });
            });

            AddStep("hold down drag bar", () =>
            {
                // distinguishes between the actual drag bar and its "underlay shadow".
                dragArea = this.ChildrenOfType<DragArea>().Single(bar => bar.HandlePositionalInput);
                InputManager.MoveMouseTo(dragArea);
                InputManager.PressButton(MouseButton.Left);
            });

            AddStep("try to drag bar past start", () =>
            {
                var blueprint = this.ChildrenOfType<TimelineHitObjectBlueprint>().Single();
                InputManager.MoveMouseTo(blueprint.SelectionQuad.TopLeft - new Vector2(100, 0));
                InputManager.ReleaseButton(MouseButton.Left);
            });

            AddAssert("object has non-zero duration", () => EditorBeatmap.HitObjects.OfType<IHasDuration>().Single().Duration > 0);
        }

        [Test]
        public void TestMultipleDurationObjects()
        {
            DragArea dragArea;
            double lastTime = double.PositiveInfinity;

            AddStep("add object with multiple durations", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.Add(new TestMultipleRelativeTimeHitObject(2700, 500, 3));
            });

            AddStep("hold down last drag bar", () =>
            {
                // distinguishes between the actual drag bar and its "underlay shadow".
                dragArea = this.ChildrenOfType<DragArea>().OrderBy(bar => bar.X).Last(bar => bar.HandlePositionalInput);
                InputManager.MoveMouseTo(dragArea);
                InputManager.PressButton(MouseButton.Left);
                lastTime = EditorBeatmap.HitObjects.OfType<IHasDuration>().Single().Duration;
            });

            AddStep("drag bar to the right", () =>
            {
                var blueprint = this.ChildrenOfType<TimelineHitObjectBlueprint>().Single();
                InputManager.MoveMouseTo(blueprint.SelectionQuad.TopRight + new Vector2(100, 0));
            });

            AddStep("release hold", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("object has longer duration", () => EditorBeatmap.HitObjects.OfType<IHasDuration>().Single().Duration > lastTime);

            AddStep("hold down second to last drag bar", () =>
            {
                // distinguishes between the actual drag bar and its "underlay shadow".
                dragArea = this.ChildrenOfType<DragArea>().OrderBy(bar => bar.X).SkipLast(1).Last(bar => bar.HandlePositionalInput);
                InputManager.MoveMouseTo(dragArea);
                InputManager.PressButton(MouseButton.Left);
                lastTime = EditorBeatmap.HitObjects.OfType<IHasMultipleRelativeTimes>().Single().TimeObjects[^2].RelativeTime;
            });

            AddStep("drag bar to the right", () => InputManager.MoveMouseTo(InputManager.CurrentState.Mouse.Position + new Vector2(100, 0)));

            AddStep("release hold", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("object has later time", () => EditorBeatmap.HitObjects.OfType<IHasMultipleRelativeTimes>().Single().TimeObjects[^2].RelativeTime > lastTime);
        }

        public class TestMultipleRelativeTimeHitObject : OsuHitObject, IHasMultipleRelativeTimes, IHasMultipleComboInformation
        {
            public double EndTime => StartTime + Duration;
            public double Duration { get => comboTimeObjects.Last().RelativeTime; set => comboTimeObjects.Last().RelativeTime = value; }
            public IReadOnlyList<IHasComboInformationAndRelativeTime> ComboObjects => comboTimeObjects;
            public IReadOnlyList<IHasRelativeTime> TimeObjects => comboTimeObjects;
            public event Action TimesUpdates;

            private readonly BindableList<TestComboObject> comboTimeObjects = new BindableList<TestComboObject>();

            public TestMultipleRelativeTimeHitObject(double start, double step, int count)
            {
                StartTime = start;

                for (int i = 1; i <= count; i++)
                {
                    comboTimeObjects.Add(new TestComboObject { NewCombo = true, RelativeTime = i * step });
                }

                comboTimeObjects.BindCollectionChanged((_, _) => TimesUpdates?.Invoke());
            }
        }

        public class TestComboObject : IHasComboInformationAndRelativeTime
        {
            public Bindable<int> IndexInCurrentComboBindable { get; } = new BindableInt();
            public int IndexInCurrentCombo { get; set; }
            public Bindable<int> ComboIndexBindable { get; } = new BindableInt();
            public int ComboIndex { get; set; }
            public Bindable<int> ComboIndexWithOffsetsBindable { get; } = new BindableInt();
            public int ComboIndexWithOffsets { get; set; }
            public bool NewCombo { get; set; }
            public Bindable<bool> LastInComboBindable { get; } = new BindableBool();
            public bool LastInCombo { get; set; }
            public int ComboOffset => 0;
            public double RelativeTime { get; set; }
        }
    }
}
