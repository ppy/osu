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
    public partial class TestSceneBeatmapAttributeText : OsuTestScene
    {
        private readonly BeatmapAttributeText text;

        public TestSceneBeatmapAttributeText()
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

        [TestCase(BeatmapAttribute.CircleSize, "Circle Size: 1.00")]
        [TestCase(BeatmapAttribute.HPDrain, "HP Drain: 2.00")]
        [TestCase(BeatmapAttribute.Accuracy, "Accuracy: 3.00")]
        [TestCase(BeatmapAttribute.ApproachRate, "Approach Rate: 4.00")]
        [TestCase(BeatmapAttribute.Title, "Title: _Title")]
        [TestCase(BeatmapAttribute.Artist, "Artist: _Artist")]
        [TestCase(BeatmapAttribute.Creator, "Creator: _Creator")]
        [TestCase(BeatmapAttribute.DifficultyName, "Difficulty: _Difficulty")]
        [TestCase(BeatmapAttribute.Source, "Source: _Source")]
        [TestCase(BeatmapAttribute.RankedStatus, "Beatmap Status: Loved")]
        public void TestAttributeDisplay(BeatmapAttribute attribute, string expectedText)
        {
            AddStep($"set attribute: {attribute}", () => text.Attribute.Value = attribute);
            AddAssert("check correct text", getText, () => Is.EqualTo(expectedText));
        }

        [Test]
        public void TestChangeBeatmap()
        {
            AddStep("set title attribute", () => text.Attribute.Value = BeatmapAttribute.Title);
            AddAssert("check initial title", getText, () => Is.EqualTo("Title: _Title"));

            AddStep("change to beatmap with another title", () => Beatmap.Value = CreateWorkingBeatmap(new TestBeatmap(new OsuRuleset().RulesetInfo)
            {
                BeatmapInfo =
                {
                    Metadata =
                    {
                        Title = "Another"
                    }
                }
            }));

            AddAssert("check new title", getText, () => Is.EqualTo("Title: Another"));
        }

        private string getText() => text.ChildrenOfType<SpriteText>().Single().Text.ToString();
    }
}
