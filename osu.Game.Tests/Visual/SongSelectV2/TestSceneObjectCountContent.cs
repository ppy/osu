// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneObjectCountContent : SongSelectComponentsTestScene
    {
        private ObjectCountContent? objectCountContent;
        private float relativeWidth;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddSliderStep("change relative width", 0, 1f, 0.5f, v =>
            {
                if (objectCountContent != null)
                    objectCountContent.Width = v;

                relativeWidth = v;
            });
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("set content", () =>
            {
                Child = objectCountContent = new ObjectCountContent
                {
                    Width = relativeWidth,
                };
            });
        }

        [Test]
        public void TestLocalBeatmap()
        {
            const int circle_count = 100;
            const int slider_count = 50;
            const int spinner_count = 2;

            AddStep("set beatmap", () =>
            {
                var beatmap = new Beatmap();

                for (int i = 0; i < circle_count; i++)
                    beatmap.HitObjects.Add(new HitCircle());

                for (int i = 0; i < slider_count; i++)
                    beatmap.HitObjects.Add(new Slider());

                for (int i = 0; i < spinner_count; i++)
                    beatmap.HitObjects.Add(new Spinner());

                Beatmap.Value = CreateWorkingBeatmap(beatmap);
            });

            AddAssert("first value is set", () => getBarValueAt(0), () => Is.EqualTo(circle_count));
            AddAssert("second value is set", () => getBarValueAt(1), () => Is.EqualTo(slider_count));
            AddAssert("third value is set", () => getBarValueAt(2), () => Is.EqualTo(spinner_count));
        }

        [Test]
        public void TestAPIBeatmap()
        {
            const int circle_count = 123;
            const int slider_count = 234;
            const int spinner_count = 3;

            AddStep("set beatmap", () => BeatmapInfo.Value = new APIBeatmap
            {
                CircleCount = circle_count,
                SliderCount = slider_count,
                SpinnerCount = spinner_count,
            });

            AddAssert("first value is set", () => getBarValueAt(0), () => Is.EqualTo(circle_count));
            AddAssert("second value is set", () => getBarValueAt(1), () => Is.EqualTo(slider_count));
            AddAssert("third value is set", () => getBarValueAt(2), () => Is.EqualTo(spinner_count));
        }

        [Test]
        public void TestNullBeatmap()
        {
            AddStep("set beatmap", () => BeatmapInfo.Value = null);
        }

        private float getBarValueAt(int index) => objectCountContent.ChildrenOfType<BarStatisticRow>().ElementAt(index).Value.baseValue;
    }
}
