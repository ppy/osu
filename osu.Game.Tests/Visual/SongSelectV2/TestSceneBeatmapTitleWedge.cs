// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Visual.SongSelect;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneBeatmapTitleWedge : SongSelectComponentsTestScene
    {
        private RulesetStore rulesets = null!;

        private BeatmapTitleWedge titleWedge = null!;
        private BeatmapTitleWedge.DifficultyDisplay difficultyDisplay => titleWedge.ChildrenOfType<BeatmapTitleWedge.DifficultyDisplay>().Single();

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            this.rulesets = rulesets;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddRange(new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        titleWedge = new BeatmapTitleWedge
                        {
                            State = { Value = Visibility.Visible },
                        },
                    },
                }
            });

            AddSliderStep("change star difficulty", 0, 11.9, 4.18, v =>
            {
                difficultyDisplay.ChildrenOfType<StarRatingDisplay>().Single().Current.Value = new StarDifficulty(v, 0);
            });
        }

        [Test]
        public void TestRulesetChange()
        {
            selectBeatmap(Beatmap.Value.Beatmap);

            AddWaitStep("wait for select", 3);

            foreach (var rulesetInfo in rulesets.AvailableRulesets)
            {
                var testBeatmap = TestSceneBeatmapInfoWedge.CreateTestBeatmap(rulesetInfo);

                setRuleset(rulesetInfo);
                selectBeatmap(testBeatmap);
            }
        }

        [Test]
        public void TestNullBeatmap()
        {
            selectBeatmap(null);
            AddAssert("check default title", () => titleWedge.DisplayedTitle == Beatmap.Default.BeatmapInfo.Metadata.Title);
            AddAssert("check default artist", () => titleWedge.DisplayedArtist == Beatmap.Default.BeatmapInfo.Metadata.Artist);
            AddAssert("check no statistics", () => difficultyDisplay.ChildrenOfType<BeatmapTitleWedge.DifficultyStatisticsDisplay>().All(d => !d.Statistics.Any()));
        }

        [Test]
        public void TestBPMUpdates()
        {
            const double bpm = 120;
            IBeatmap beatmap = TestSceneBeatmapInfoWedge.CreateTestBeatmap(new OsuRuleset().RulesetInfo);
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 60 * 1000 / bpm });

            OsuModDoubleTime doubleTime = null!;

            selectBeatmap(beatmap);
            checkDisplayedBPM($"{bpm}");

            AddStep("select DT", () => SelectedMods.Value = new[] { doubleTime = new OsuModDoubleTime() });
            checkDisplayedBPM($"{bpm * 1.5f}");

            AddStep("change DT rate", () => doubleTime.SpeedChange.Value = 2);
            checkDisplayedBPM($"{bpm * 2}");

            AddStep("select HT", () => SelectedMods.Value = new[] { new OsuModHalfTime() });
            checkDisplayedBPM($"{bpm * 0.75f}");
        }

        [Test]
        public void TestWedgeVisibility()
        {
            AddStep("hide", () => { titleWedge.Hide(); });
            AddWaitStep("wait for hide", 3);
            AddAssert("check visibility", () => titleWedge.Alpha == 0);
            AddStep("show", () => { titleWedge.Show(); });
            AddWaitStep("wait for show", 1);
            AddAssert("check visibility", () => titleWedge.Alpha > 0);
        }

        [TestCase(120, 125, null, "120-125 (mostly 120)")]
        [TestCase(120, 120.6, null, "120-121 (mostly 120)")]
        [TestCase(120, 120.4, null, "120")]
        [TestCase(120, 120.6, "DT", "180-182 (mostly 180)")]
        [TestCase(120, 120.4, "DT", "180")]
        public void TestVaryingBPM(double commonBpm, double otherBpm, string? mod, string expectedDisplay)
        {
            IBeatmap beatmap = TestSceneBeatmapInfoWedge.CreateTestBeatmap(new OsuRuleset().RulesetInfo);
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 60 * 1000 / commonBpm });
            beatmap.ControlPointInfo.Add(100, new TimingControlPoint { BeatLength = 60 * 1000 / otherBpm });
            beatmap.ControlPointInfo.Add(200, new TimingControlPoint { BeatLength = 60 * 1000 / commonBpm });

            if (mod != null)
                AddStep($"select {mod}", () => SelectedMods.Value = new[] { Ruleset.Value.CreateInstance().CreateModFromAcronym(mod) });

            selectBeatmap(beatmap);
            checkDisplayedBPM(expectedDisplay);
        }

        private void setRuleset(RulesetInfo rulesetInfo)
        {
            AddStep("set ruleset", () => Ruleset.Value = rulesetInfo);
        }

        private void selectBeatmap(IBeatmap? b)
        {
            AddStep($"select {b?.Metadata.Title ?? "null"} beatmap", () =>
            {
                Beatmap.Value = b == null ? Beatmap.Default : CreateWorkingBeatmap(b);
            });
        }

        private void checkDisplayedBPM(string target)
        {
            AddUntilStep($"displayed bpm is {target}", () =>
            {
                var label = titleWedge.ChildrenOfType<BeatmapTitleWedge.Statistic>().Single(l => l.TooltipText == BeatmapsetsStrings.ShowStatsBpm);
                return label.Text == target;
            });
        }
    }
}
