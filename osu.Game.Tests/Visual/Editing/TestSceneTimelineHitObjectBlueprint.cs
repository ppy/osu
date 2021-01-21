using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osuTK;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneTimelineHitObjectBlueprint : TimelineTestScene
    {
        private Spinner spinner;
        private TimelineHitObjectBlueprint blueprint;

        public TestSceneTimelineHitObjectBlueprint()
        {
            var spinner = new Spinner
            {
                Position = new Vector2(256, 256),
                StartTime = -1000,
                EndTime = 2000
            };

            spinner.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 2 });
            Add(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.5f),
                Child = _ = new DrawableSpinner(spinner)
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Clock.Seek(10000);
        }

        public override Drawable CreateTestComponent() => blueprint = new TimelineHitObjectBlueprint(spinner);

        [Test]
        public void TestDisallowZeroLengthSpinners()
        {

        }
    }
}
