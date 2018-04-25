// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Select;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseBeatmapInfoWedge : OsuTestCase
    {
        private RulesetStore rulesets;
        private TestBeatmapInfoWedge infoWedge;
        private readonly List<Beatmap> beatmaps = new List<Beatmap>();
        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, RulesetStore rulesets)
        {
            this.rulesets = rulesets;

            beatmap.BindTo(game.Beatmap);
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
                infoWedge.State = Visibility.Visible;
                infoWedge.UpdateBeatmap(beatmap);
            });

            // select part is redundant, but wait for load isn't
            selectBeatmap(beatmap.Value.Beatmap);

            AddWaitStep(3);

            AddStep("hide", () => { infoWedge.State = Visibility.Hidden; });

            AddWaitStep(3);

            AddStep("show", () => { infoWedge.State = Visibility.Visible; });

            foreach (var rulesetInfo in rulesets.AvailableRulesets)
            {
                var ruleset = rulesetInfo.CreateInstance();
                var testBeatmap = createTestBeatmap(rulesetInfo);

                beatmaps.Add(testBeatmap);

                selectBeatmap(testBeatmap);

                // TODO: adjust cases once more info is shown for other gamemodes
                switch (ruleset)
                {
                    case OsuRuleset osu:
                        testOsuBeatmap(osu);
                        testInfoLabels(5);
                        break;
                    default:
                        testInfoLabels(2);
                        break;
                }
            }

            testNullBeatmap();
        }

        private void testOsuBeatmap(OsuRuleset ruleset)
        {
            AddAssert("check version", () => infoWedge.Info.VersionLabel.Text == $"{ruleset.ShortName}Version");
            AddAssert("check title", () => infoWedge.Info.TitleLabel.Text == $"{ruleset.ShortName}Source — {ruleset.ShortName}Title");
            AddAssert("check artist", () => infoWedge.Info.ArtistLabel.Text == $"{ruleset.ShortName}Artist");
            AddAssert("check author", () => infoWedge.Info.MapperContainer.Children.OfType<OsuSpriteText>().Any(s => s.Text == $"{ruleset.ShortName}Author"));
        }

        private void testInfoLabels(int expectedCount)
        {
            AddAssert("check infolabels exists", () => infoWedge.Info.InfoLabelContainer.Children.Any());
            AddAssert("check infolabels count", () => infoWedge.Info.InfoLabelContainer.Children.Count == expectedCount);
        }

        private void testNullBeatmap()
        {
            selectNullBeatmap();
            AddAssert("check empty version", () => string.IsNullOrEmpty(infoWedge.Info.VersionLabel.Text));
            AddAssert("check default title", () => infoWedge.Info.TitleLabel.Text == beatmap.Default.BeatmapInfo.Metadata.Title);
            AddAssert("check default artist", () => infoWedge.Info.ArtistLabel.Text == beatmap.Default.BeatmapInfo.Metadata.Artist);
            AddAssert("check empty author", () => !infoWedge.Info.MapperContainer.Children.Any());
            AddAssert("check no infolabels", () => !infoWedge.Info.InfoLabelContainer.Children.Any());
        }

        private void selectBeatmap(Beatmap b)
        {
            BeatmapInfoWedge.BufferedWedgeInfo infoBefore = null;

            AddStep($"select {b.Metadata.Title} beatmap", () =>
            {
                infoBefore = infoWedge.Info;
                infoWedge.UpdateBeatmap(beatmap.Value = new TestWorkingBeatmap(b));
            });

            AddUntilStep(() => infoWedge.Info != infoBefore, "wait for async load");
        }

        private void selectNullBeatmap()
        {
            AddStep("select null beatmap", () =>
            {
                beatmap.Value = beatmap.Default;
                infoWedge.UpdateBeatmap(beatmap);
            });
        }

        private Beatmap createTestBeatmap(RulesetInfo ruleset)
        {
            List<HitObject> objects = new List<HitObject>();
            for (double i = 0; i < 50000; i += 1000)
                objects.Add(new HitObject { StartTime = i });

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
                    StarDifficulty = 6,
                    Version = $"{ruleset.ShortName}Version"
                },
                HitObjects = objects
            };
        }

        private class TestBeatmapInfoWedge : BeatmapInfoWedge
        {
            public new BufferedWedgeInfo Info => base.Info;
        }
    }
}
