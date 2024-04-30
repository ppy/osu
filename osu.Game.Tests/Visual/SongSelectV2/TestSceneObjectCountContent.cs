// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
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
            AddStep("set beatmap", () =>
            {
                var beatmap = new Beatmap();

                for (int i = 0; i < 100; i++)
                {
                    beatmap.HitObjects.Add(new HitCircle());

                    if (i % 2 == 0)
                        beatmap.HitObjects.Add(new Slider());

                    if (i % 50 == 0)
                        beatmap.HitObjects.Add(new Spinner());
                }

                Beatmap.Value = CreateWorkingBeatmap(beatmap);
            });
        }

        [Test]
        public void TestAPIBeatmap()
        {
            AddStep("set beatmap", () => BeatmapInfo.Value = new APIBeatmap
            {
                CircleCount = 123,
                SliderCount = 234,
                SpinnerCount = 3,
            });
        }

        [Test]
        public void TestNullBeatmap()
        {
            AddStep("set beatmap", () => BeatmapInfo.Value = null);
        }
    }
}
