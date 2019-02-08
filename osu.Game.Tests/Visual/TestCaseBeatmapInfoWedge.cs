﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Screens.Select;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseBeatmapInfoWedge : OsuTestCase
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
                infoWedge.State = Visibility.Visible;
                infoWedge.Beatmap = Beatmap;
            });

            // select part is redundant, but wait for load isn't
            selectBeatmap(Beatmap.Value.Beatmap);

            AddWaitStep(3);

            AddStep("hide", () => { infoWedge.State = Visibility.Hidden; });

            AddWaitStep(3);

            AddStep("show", () => { infoWedge.State = Visibility.Visible; });

            foreach (var rulesetInfo in rulesets.AvailableRulesets)
            {
                var instance = rulesetInfo.CreateInstance();
                var testBeatmap = createTestBeatmap(rulesetInfo);

                beatmaps.Add(testBeatmap);

                AddStep("set ruleset", () => Ruleset.Value = rulesetInfo);

                selectBeatmap(testBeatmap);

                testBeatmapLabels(instance);

                // TODO: adjust cases once more info is shown for other gamemodes
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

            testNullBeatmap();
        }

        private void testBeatmapLabels(Ruleset ruleset)
        {
            AddAssert("check version", () => infoWedge.Info.VersionLabel.Text == $"{ruleset.ShortName}Version");
            AddAssert("check title", () => infoWedge.Info.TitleLabel.Text == $"{ruleset.ShortName}Source — {ruleset.ShortName}Title");
            AddAssert("check artist", () => infoWedge.Info.ArtistLabel.Text == $"{ruleset.ShortName}Artist");
            AddAssert("check author", () => infoWedge.Info.MapperContainer.Children.OfType<OsuSpriteText>().Any(s => s.Text == $"{ruleset.ShortName}Author"));
        }

        private void testInfoLabels(int expectedCount)
        {
            AddAssert("check info labels exists", () => infoWedge.Info.InfoLabelContainer.Children.Any());
            AddAssert("check info labels count", () => infoWedge.Info.InfoLabelContainer.Children.Count == expectedCount);
        }

        private void testNullBeatmap()
        {
            selectBeatmap(null);
            AddAssert("check empty version", () => string.IsNullOrEmpty(infoWedge.Info.VersionLabel.Text));
            AddAssert("check default title", () => infoWedge.Info.TitleLabel.Text == Beatmap.Default.BeatmapInfo.Metadata.Title);
            AddAssert("check default artist", () => infoWedge.Info.ArtistLabel.Text == Beatmap.Default.BeatmapInfo.Metadata.Artist);
            AddAssert("check empty author", () => !infoWedge.Info.MapperContainer.Children.Any());
            AddAssert("check no info labels", () => !infoWedge.Info.InfoLabelContainer.Children.Any());
        }

        private void selectBeatmap([CanBeNull] IBeatmap b)
        {
            BeatmapInfoWedge.BufferedWedgeInfo infoBefore = null;

            AddStep($"select {b?.Metadata.Title ?? "null"} beatmap", () =>
            {
                infoBefore = infoWedge.Info;
                infoWedge.Beatmap = Beatmap.Value = b == null ? Beatmap.Default : new TestWorkingBeatmap(b);
            });

            AddUntilStep(() => infoWedge.Info != infoBefore, "wait for async load");
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
                    StarDifficulty = 6,
                    Version = $"{ruleset.ShortName}Version",
                    BaseDifficulty = new BeatmapDifficulty()
                },
                HitObjects = objects
            };
        }

        private class TestBeatmapInfoWedge : BeatmapInfoWedge
        {
            public new BufferedWedgeInfo Info => base.Info;
        }

        private class TestHitObject : HitObject, IHasPosition
        {
            public float X { get; } = 0;
            public float Y { get; } = 0;
            public Vector2 Position { get; } = Vector2.Zero;
        }
    }
}
