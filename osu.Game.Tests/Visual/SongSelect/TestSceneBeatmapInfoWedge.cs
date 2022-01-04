// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Taiko;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelect
{
    [TestFixture]
    public class TestSceneBeatmapInfoWedge : OsuTestScene
    {
        private RulesetStore rulesets;
        private TestBeatmapInfoWedge infoWedge;
        private readonly List<IBeatmap> beatmaps = new List<IBeatmap>();

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            this.rulesets = rulesets;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(infoWedge = new TestBeatmapInfoWedge
            {
                Size = new Vector2(0.5f, 245),
                RelativeSizeAxes = Axes.X,
                Margin = new MarginPadding { Top = 20 }
            });

            AddStep("show", () =>
            {
                infoWedge.Show();
                infoWedge.Beatmap = Beatmap.Value;
            });

            // select part is redundant, but wait for load isn't
            selectBeatmap(Beatmap.Value.Beatmap);

            AddWaitStep("wait for select", 3);

            AddStep("hide", () => { infoWedge.Hide(); });

            AddWaitStep("wait for hide", 3);

            AddStep("show", () => { infoWedge.Show(); });

            AddSliderStep("change star difficulty", 0, 11.9, 5.55, v =>
            {
                foreach (var hasCurrentValue in infoWedge.Info.ChildrenOfType<IHasCurrentValue<StarDifficulty>>())
                    hasCurrentValue.Current.Value = new StarDifficulty(v, 0);
            });

            foreach (var rulesetInfo in rulesets.AvailableRulesets)
            {
                var instance = rulesetInfo.CreateInstance();
                var testBeatmap = createTestBeatmap(rulesetInfo);

                beatmaps.Add(testBeatmap);

                setRuleset(rulesetInfo);

                selectBeatmap(testBeatmap);

                testBeatmapLabels(instance);

                switch (instance)
                {
                    case OsuRuleset _:
                        testInfoLabels(5);
                        break;

                    case TaikoRuleset _:
                        testInfoLabels(5);
                        break;

                    case CatchRuleset _:
                        testInfoLabels(5);
                        break;

                    case ManiaRuleset _:
                        testInfoLabels(4);
                        break;

                    default:
                        testInfoLabels(2);
                        break;
                }
            }
        }

        private void testBeatmapLabels(Ruleset ruleset)
        {
            AddAssert("check version", () => infoWedge.Info.VersionLabel.Current.Value == $"{ruleset.ShortName}Version");
            AddAssert("check title", () => infoWedge.Info.TitleLabel.Current.Value == $"{ruleset.ShortName}Source — {ruleset.ShortName}Title");
            AddAssert("check artist", () => infoWedge.Info.ArtistLabel.Current.Value == $"{ruleset.ShortName}Artist");
            AddAssert("check author", () => infoWedge.Info.MapperContainer.ChildrenOfType<OsuSpriteText>().Any(s => s.Current.Value == $"{ruleset.ShortName}Author"));
        }

        private void testInfoLabels(int expectedCount)
        {
            AddAssert("check info labels exists", () => infoWedge.Info.ChildrenOfType<BeatmapInfoWedge.WedgeInfoText.InfoLabel>().Any());
            AddAssert("check info labels count", () => infoWedge.Info.ChildrenOfType<BeatmapInfoWedge.WedgeInfoText.InfoLabel>().Count() == expectedCount);
        }

        [Test]
        public void TestNullBeatmap()
        {
            selectBeatmap(null);
            AddAssert("check empty version", () => string.IsNullOrEmpty(infoWedge.Info.VersionLabel.Current.Value));
            AddAssert("check default title", () => infoWedge.Info.TitleLabel.Current.Value == Beatmap.Default.BeatmapInfo.Metadata.Title);
            AddAssert("check default artist", () => infoWedge.Info.ArtistLabel.Current.Value == Beatmap.Default.BeatmapInfo.Metadata.Artist);
            AddAssert("check empty author", () => !infoWedge.Info.MapperContainer.ChildrenOfType<OsuSpriteText>().Any());
            AddAssert("check no info labels", () => !infoWedge.Info.ChildrenOfType<BeatmapInfoWedge.WedgeInfoText.InfoLabel>().Any());
        }

        [Test]
        public void TestTruncation()
        {
            selectBeatmap(createLongMetadata());
        }

        [Test]
        public void TestBPMUpdates()
        {
            const float bpm = 120;
            IBeatmap beatmap = createTestBeatmap(new OsuRuleset().RulesetInfo);
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 60 * 1000 / bpm });

            OsuModDoubleTime doubleTime = null;

            selectBeatmap(beatmap);
            checkDisplayedBPM(bpm);

            AddStep("select DT", () => SelectedMods.Value = new[] { doubleTime = new OsuModDoubleTime() });
            checkDisplayedBPM(bpm * 1.5f);

            AddStep("change DT rate", () => doubleTime.SpeedChange.Value = 2);
            checkDisplayedBPM(bpm * 2);

            void checkDisplayedBPM(float target) =>
                AddUntilStep($"displayed bpm is {target}", () => this.ChildrenOfType<BeatmapInfoWedge.WedgeInfoText.InfoLabel>().Any(
                    label => label.Statistic.Name == "BPM" && label.Statistic.Content == target.ToString(CultureInfo.InvariantCulture)));
        }

        private void setRuleset(RulesetInfo rulesetInfo)
        {
            Container containerBefore = null;

            AddStep("set ruleset", () =>
            {
                // wedge content is only refreshed if the ruleset changes, so only wait for load in that case.
                if (!rulesetInfo.Equals(Ruleset.Value))
                    containerBefore = infoWedge.DisplayedContent;

                Ruleset.Value = rulesetInfo;
            });

            AddUntilStep("wait for async load", () => infoWedge.DisplayedContent != containerBefore);
        }

        private void selectBeatmap([CanBeNull] IBeatmap b)
        {
            Container containerBefore = null;

            AddStep($"select {b?.Metadata.Title ?? "null"} beatmap", () =>
            {
                containerBefore = infoWedge.DisplayedContent;
                infoWedge.Beatmap = Beatmap.Value = b == null ? Beatmap.Default : CreateWorkingBeatmap(b);
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
                        AuthorString = $"{ruleset.ShortName}Author",
                        Artist = $"{ruleset.ShortName}Artist",
                        Source = $"{ruleset.ShortName}Source",
                        Title = $"{ruleset.ShortName}Title"
                    },
                    Ruleset = ruleset,
                    StarRating = 6,
                    DifficultyName = $"{ruleset.ShortName}Version",
                    BaseDifficulty = new BeatmapDifficulty()
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
                        AuthorString = "WWWWWWWWWWWWWWW",
                        Artist = "Verrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrry long Artist",
                        Source = "Verrrrry long Source",
                        Title = "Verrrrry long Title"
                    },
                    DifficultyName = "Verrrrrrrrrrrrrrrrrrrrrrrrrrrrry long Version",
                    Status = BeatmapOnlineStatus.Graveyard,
                },
            };
        }

        private class TestBeatmapInfoWedge : BeatmapInfoWedge
        {
            public new Container DisplayedContent => base.DisplayedContent;

            public new WedgeInfoText Info => base.Info;
        }

        private class TestHitObject : ConvertHitObject, IHasPosition
        {
            public float X => 0;
            public float Y => 0;
            public Vector2 Position { get; } = Vector2.Zero;
        }
    }
}
