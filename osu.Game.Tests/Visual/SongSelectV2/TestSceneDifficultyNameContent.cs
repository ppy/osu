// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneDifficultyNameContent : SongSelectComponentsTestScene
    {
        private DifficultyNameContent? difficultyNameContent;
        private float relativeWidth;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddSliderStep("change relative width", 0, 1f, 0.5f, v =>
            {
                if (difficultyNameContent != null)
                    difficultyNameContent.Width = v;

                relativeWidth = v;
            });
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("set content", () =>
            {
                Child = difficultyNameContent = new DifficultyNameContent
                {
                    Width = relativeWidth,
                };
            });
        }

        [Test]
        public void TestLocalBeatmap()
        {
            AddAssert("difficulty name is not set", () => LocalisableString.IsNullOrEmpty(difficultyNameContent.ChildrenOfType<TruncatingSpriteText>().Single().Text));
            AddAssert("author is not set", () => !difficultyNameContent.ChildrenOfType<LinkFlowContainer>().Single().ChildrenOfType<OsuSpriteText>().Any());

            AddStep("set beatmap", () => Beatmap.Value = CreateWorkingBeatmap(new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    DifficultyName = "really long difficulty name that gets truncated",
                    Metadata = new BeatmapMetadata
                    {
                        Author = { Username = "really long username that is autosized" },
                    },
                    OnlineID = 1,
                }
            }));

            AddAssert("difficulty name is set", () => !LocalisableString.IsNullOrEmpty(difficultyNameContent.ChildrenOfType<TruncatingSpriteText>().Single().Text));
            AddAssert("author is set", () => difficultyNameContent.ChildrenOfType<LinkFlowContainer>().Single().ChildrenOfType<OsuSpriteText>().Any());
        }

        [Test]
        public void TestAPIBeatmap()
        {
            AddAssert("difficulty name is not set", () => LocalisableString.IsNullOrEmpty(difficultyNameContent.ChildrenOfType<TruncatingSpriteText>().Single().Text));
            AddAssert("author is not set", () => !difficultyNameContent.ChildrenOfType<LinkFlowContainer>().Single().ChildrenOfType<OsuSpriteText>().Any());

            AddStep("set beatmap", () => BeatmapInfo.Value = new APIBeatmap
            {
                BeatmapSet = new APIBeatmapSet
                {
                    RelatedUsers =
                    [
                        new APIUser { Id = 1, Username = "user 1" },
                        new APIUser { Id = 2, Username = "user 2" }
                    ],
                },
                DifficultyName = "user 1's difficulty name",
                AuthorID = 1,
                OnlineID = 2,
            });

            AddAssert("difficulty name is set", () => !LocalisableString.IsNullOrEmpty(difficultyNameContent.ChildrenOfType<TruncatingSpriteText>().Single().Text));
            AddAssert("author is set to user 1", () => difficultyNameContent.ChildrenOfType<LinkFlowContainer>().Single().ChildrenOfType<OsuSpriteText>().Last().Text.ToString(), () => Is.EqualTo("1"));
        }

        [Test]
        public void TestNullBeatmap()
        {
            AddStep("set beatmap", () => BeatmapInfo.Value = null);
        }
    }
}
