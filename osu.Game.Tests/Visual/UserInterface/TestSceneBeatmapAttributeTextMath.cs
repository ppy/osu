// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Models;
using osu.Game.Rulesets.Osu;
using osu.Game.Skinning.Components;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneBeatmapAttributeTextMath : OsuTestScene
    {
        private readonly BeatmapAttributeText text;

        public TestSceneBeatmapAttributeTextMath()
        {
            Child = text = new BeatmapAttributeText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            SelectedMods.SetDefault();
            Ruleset.Value = new OsuRuleset().RulesetInfo;
            Beatmap.Value = CreateWorkingBeatmap(new TestBeatmap(new OsuRuleset().RulesetInfo)
            {
                BeatmapInfo =
                {
                    BPM = 100,
                    DifficultyName = "_Difficulty",
                    Status = BeatmapOnlineStatus.Loved,
                    Metadata =
                    {
                        Title = "_Title",
                        TitleUnicode = "_Title",
                        Artist = "_Artist",
                        ArtistUnicode = "_Artist",
                        Author = new RealmUser { Username = "_Creator" },
                        Source = "_Source",
                    },
                    Difficulty =
                    {
                        CircleSize = 1,
                        DrainRate = 2,
                        OverallDifficulty = 3,
                        ApproachRate = 4,
                    }
                }
            });
        });

        [TestCase("{CircleSize+1}", "2")]
        [TestCase("{CircleSize+-1}", "0")]
        [TestCase("{CircleSize-1}", "0")]
        [TestCase("{-CircleSize}", "-1")]
        [TestCase("{ {-CircleSize}", "{ -1")]
        [TestCase("{(-CircleSize)}", "-1")]
        [TestCase("{-(CircleSize)}", "-1")]
        [TestCase("{(-Circleize)}", "{(-Circleize)}")]
        [TestCase("{4+ApproachRate/4}", "5")]
        [TestCase("{(4+ApproachRate)/4}", "2")]
        [TestCase("{-CircleSize} {CircleSize+0} {CircleSize+}", "-1 1 {CircleSize+}")]
        [TestCase("{-CircleSize} {CircleSize +0} {CircleSize+0}", "-1 {CircleSize +0} 1")]
        [TestCase("{()()()()}", "{()()()()}")]
        [TestCase("{-()()()()}", "{-()()()()}")]
        [TestCase("{()+()+()+()}", "{()+()+()+()}")]
        [TestCase("{(ApproachRate)(ApproachRate)(ApproachRate)(ApproachRate)}", "256")]
        [TestCase("{(ApproachRate-(ApproachRate*ApproachRate*ApproachRate))}", "-60")]
        [TestCase("{1/0}", "{1/0}")]
        [TestCase("{1%0}", "{1%0}")]
        [TestCase("{(-1)+1*1*1+(-1-1)}", "-2")]
        [TestCase("{4%3}", "1")]
        [TestCase("{4%1.5}", "1")]
        [TestCase("{-1}", "-1")]
        [TestCase("{(-1)}", "-1")]
        [TestCase("{4(CircleSize)}", "4")]
        [TestCase("{(ApproachRate)(ApproachRate)}", "16")]
        [TestCase("(ApproachRate-CircleSize)(Accuracy)={(ApproachRate-CircleSize)(Accuracy)}", "(ApproachRate-CircleSize)(Accuracy)=9")]
        public void TestAttributeMathDisplay(string inputText, string expectedText)
        {
            AddStep($"set text: \"{inputText}\"", () => text.Template.Value = inputText);
            AddAssert("check correct text", getText, () => Is.EqualTo(expectedText));
        }

        private string getText() => text.ChildrenOfType<SpriteText>().Single().Text.ToString();
    }
}
