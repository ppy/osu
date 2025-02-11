// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
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
        private DifficultyNameContent? difficultyNameContent;

        [Test]
        public void TestLocalBeatmap()
        {
            AddStep("set component", () => Child = difficultyNameContent = new LocalDifficultyNameContent());

            AddAssert("difficulty name is not set", () => LocalisableString.IsNullOrEmpty(difficultyNameContent.ChildrenOfType<TruncatingSpriteText>().Single().Text));
            AddAssert("author is not set", () => LocalisableString.IsNullOrEmpty(difficultyNameContent.ChildrenOfType<OsuHoverContainer>().Single().ChildrenOfType<OsuSpriteText>().Single().Text));

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
            AddAssert("author is set", () => !LocalisableString.IsNullOrEmpty(difficultyNameContent.ChildrenOfType<OsuHoverContainer>().Single().ChildrenOfType<OsuSpriteText>().Single().Text));
        }
    }
}
