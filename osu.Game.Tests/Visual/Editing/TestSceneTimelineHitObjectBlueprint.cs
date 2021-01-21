// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osuTK;
using osuTK.Input;
using static osu.Game.Screens.Edit.Compose.Components.Timeline.TimelineHitObjectBlueprint;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneTimelineHitObjectBlueprint : TimelineTestScene
    {
        public override Drawable CreateTestComponent() => new TimelineBlueprintContainer(Composer);

        [Test]
        public void TestDisallowZeroDurationObjects()
        {
            DragBar dragBar;

            AddStep("add spinner", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.Add(new Spinner
                {
                    Position = new Vector2(256, 256),
                    StartTime = 150,
                    Duration = 500
                });
            });

            AddStep("hold down drag bar", () =>
            {
                // distinguishes between the actual drag bar and its "underlay shadow".
                dragBar = this.ChildrenOfType<DragBar>().Single(bar => bar.HandlePositionalInput);
                InputManager.MoveMouseTo(dragBar);
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
    }
}
