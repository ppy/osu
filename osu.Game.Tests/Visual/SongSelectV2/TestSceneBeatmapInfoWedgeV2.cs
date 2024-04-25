// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    [TestFixture]
    public partial class TestSceneBeatmapInfoWedgeV2 : OsuTestScene
    {
        private RulesetStore rulesets = null!;
        private TestBeatmapInfoWedgeV2 infoWedge = null!;
        private readonly List<IBeatmap> beatmaps = new List<IBeatmap>();

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            this.rulesets = rulesets;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Top = 20 },
                Child = infoWedge = new TestBeatmapInfoWedgeV2
                {
                    Width = 0.6f,
                    RelativeSizeAxes = Axes.X,
                },
            });

            AddSliderStep("change star difficulty", 0, 11.9, 5.55, v =>
            {
                foreach (var hasCurrentValue in infoWedge.ChildrenOfType<IHasCurrentValue<StarDifficulty>>())
                    hasCurrentValue.Current.Value = new StarDifficulty(v, 0);
            });
        }

        [Test]
        public void TestRulesetChange()
        {
            selectBeatmap(Beatmap.Value.Beatmap);

            AddWaitStep("wait for select", 3);

            foreach (var rulesetInfo in rulesets.AvailableRulesets)
            {
                var instance = rulesetInfo.CreateInstance();
                var testBeatmap = createTestBeatmap(rulesetInfo);

                beatmaps.Add(testBeatmap);

                setRuleset(rulesetInfo);

                selectBeatmap(testBeatmap);

                testBeatmapLabels(instance);
            }
        }

        [Test]
        public void TestWedgeVisibility()
        {
            AddStep("hide", () => { infoWedge.Hide(); });
            AddWaitStep("wait for hide", 3);
            AddAssert("check visibility", () => infoWedge.Alpha == 0);
            AddStep("show", () => { infoWedge.Show(); });
            AddWaitStep("wait for show", 1);
            AddAssert("check visibility", () => infoWedge.Alpha > 0);
        }

        private void testBeatmapLabels(Ruleset ruleset)
        {
            AddAssert("check title", () => infoWedge.Info!.TitleLabel.Current.Value == $"{ruleset.ShortName}Title");
            AddAssert("check artist", () => infoWedge.Info!.ArtistLabel.Current.Value == $"{ruleset.ShortName}Artist");
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("reset mods", () => SelectedMods.SetDefault());
        }

        [Test]
        public void TestTruncation()
        {
            selectBeatmap(createLongMetadata());
        }

        [Test]
        public void TestNullBeatmapWithBackground()
        {
            selectBeatmap(null);
            AddAssert("check default title", () => infoWedge.Info!.TitleLabel.Current.Value == Beatmap.Default.BeatmapInfo.Metadata.Title);
            AddAssert("check default artist", () => infoWedge.Info!.ArtistLabel.Current.Value == Beatmap.Default.BeatmapInfo.Metadata.Artist);
            AddAssert("check no info labels", () => !infoWedge.Info.ChildrenOfType<BeatmapInfoWedge.WedgeInfoText.InfoLabel>().Any());
        }

        private void setRuleset(RulesetInfo rulesetInfo)
        {
            Container? containerBefore = null;

            AddStep("set ruleset", () =>
            {
                // wedge content is only refreshed if the ruleset changes, so only wait for load in that case.
                if (!rulesetInfo.Equals(Ruleset.Value))
                    containerBefore = infoWedge.DisplayedContent;

                Ruleset.Value = rulesetInfo;
            });

            AddUntilStep("wait for async load", () => infoWedge.DisplayedContent != containerBefore);
        }

        private void selectBeatmap(IBeatmap? b)
        {
            Container? containerBefore = null;

            AddStep($"select {b?.Metadata.Title ?? "null"} beatmap", () =>
            {
                containerBefore = infoWedge.DisplayedContent;
                infoWedge.Beatmap = Beatmap.Value = b == null ? Beatmap.Default : CreateWorkingBeatmap(b);
                infoWedge.Show();
            });

            AddUntilStep("wait for async load", () => infoWedge.DisplayedContent != containerBefore);
        }

        private IBeatmap createTestBeatmap(RulesetInfo ruleset)
        {
            List<HitObject> objects = new List<HitObject>();
            for (double i = 0; i < 50000; i += 1000)
                objects.Add(new TestHitObject { StartTime = i });

            return new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Author = { Username = $"{ruleset.ShortName}Author" },
                        Artist = $"{ruleset.ShortName}Artist",
                        Source = $"{ruleset.ShortName}Source",
                        Title = $"{ruleset.ShortName}Title"
                    },
                    Ruleset = ruleset,
                    StarRating = 6,
                    DifficultyName = $"{ruleset.ShortName}Version",
                    Difficulty = new BeatmapDifficulty()
                },
                HitObjects = objects
            };
        }

        private IBeatmap createLongMetadata()
        {
            return new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Author = { Username = "WWWWWWWWWWWWWWW" },
                        Artist = "Verrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrry long Artist",
                        Source = "Verrrrry long Source",
                        Title = "Verrrrry long Title"
                    },
                    DifficultyName = "Verrrrrrrrrrrrrrrrrrrrrrrrrrrrry long Version",
                    Status = BeatmapOnlineStatus.Graveyard,
                },
            };
        }

        private partial class TestBeatmapInfoWedgeV2 : BeatmapInfoWedgeV2
        {
            public new Container? DisplayedContent => base.DisplayedContent;
            public new WedgeInfoText? Info => base.Info;
        }

        private class TestHitObject : ConvertHitObject, IHasPosition
        {
            public float X => 0;
            public float Y => 0;
            public Vector2 Position { get; } = Vector2.Zero;
        }
    }
}
