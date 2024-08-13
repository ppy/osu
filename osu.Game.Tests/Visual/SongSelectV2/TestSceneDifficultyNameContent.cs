// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.SelectV2.Wedge;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneDifficultyNameContent : SongSelectComponentsTestScene
    {
        private Container? content;
        private DifficultyNameContent? difficultyNameContent;
        private float relativeWidth;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddSliderStep("change relative width", 0, 1f, 0.5f, v =>
            {
                if (content != null)
                    content.Width = v;

                relativeWidth = v;
            });
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("set content", () =>
            {
                Child = content = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(10),
                    Width = relativeWidth,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourProvider.Background5,
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding(10),
                            Child = difficultyNameContent = new DifficultyNameContent(),
                        }
                    }
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
        public void TestNullBeatmap()
        {
            AddStep("set beatmap", () => BeatmapInfo.Value = null);
        }
    }
}
