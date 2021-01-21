using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osuTK;
using osuTK.Input;
using static osu.Game.Screens.Edit.Compose.Components.Timeline.TimelineHitObjectBlueprint;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneTimelineHitObjectBlueprint : TimelineTestScene
    {
        private Spinner spinner;

        public TestSceneTimelineHitObjectBlueprint()
        {
            spinner = new Spinner
            {
                Position = new Vector2(256, 256),
                StartTime = -1000,
                EndTime = 2000
            };

            spinner.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 2 });
        }

        public override Drawable CreateTestComponent() => new TimelineHitObjectBlueprint(spinner);

        [Test]
        public void TestDisallowZeroLengthSpinners()
        {
            DragBar dragBar = this.ChildrenOfType<DragBar>().First();
            Circle circle = this.ChildrenOfType<Circle>().First();
            InputManager.MoveMouseTo(dragBar.ScreenSpaceDrawQuad.TopRight);
            AddStep("drag dragbar to hit object", () =>
            {
                InputManager.PressButton(MouseButton.Left);
                InputManager.MoveMouseTo(circle.ScreenSpaceDrawQuad.TopLeft);
                InputManager.ReleaseButton(MouseButton.Left);
            });
        }
    }
}
