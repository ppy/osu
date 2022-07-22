// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestScenePlayerLocalScoreImport : PlayerTestScene
    {
        private BeatmapManager beatmaps = null!;
        private RulesetStore rulesets = null!;

        private BeatmapSetInfo? importedSet;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, rulesets, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(new ScoreManager(rulesets, () => beatmaps, LocalStorage, Realm, Scheduler, API));
            Dependencies.Cache(Realm);
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("import beatmap", () =>
            {
                beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
                importedSet = beatmaps.GetAllUsableBeatmapSets().First();
            });
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => beatmaps.GetWorkingBeatmap(importedSet?.Beatmaps.First()).Beatmap;

        private Ruleset? customRuleset;

        protected override Ruleset CreatePlayerRuleset() => customRuleset ?? new OsuRuleset();

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new TestPlayer(false);

        protected override bool HasCustomSteps => true;

        protected override bool AllowFail => allowFail;

        private bool allowFail;

        [SetUp]
        public void SetUp()
        {
            allowFail = false;
            customRuleset = null;
        }

        [Test]
        public void TestSaveFailedReplay()
        {
            AddStep("allow fail", () => allowFail = true);

            CreateTest();

            AddUntilStep("fail screen displayed", () => Player.ChildrenOfType<FailOverlay>().First().State.Value == Visibility.Visible);
            AddUntilStep("score not in database", () => Realm.Run(r => r.Find<ScoreInfo>(Player.Score.ScoreInfo.ID) == null));
            AddStep("click save button", () => Player.ChildrenOfType<SaveFailedScoreButton>().First().ChildrenOfType<OsuClickableContainer>().First().TriggerClick());
            AddUntilStep("score not in database", () => Realm.Run(r => r.Find<ScoreInfo>(Player.Score.ScoreInfo.ID) != null));
        }

        [Test]
        public void TestLastPlayedUpdated()
        {
            DateTimeOffset? getLastPlayed() => Realm.Run(r => r.Find<BeatmapInfo>(Beatmap.Value.BeatmapInfo.ID)?.LastPlayed);

            AddAssert("last played is null", () => getLastPlayed() == null);

            CreateTest();

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);
            AddUntilStep("wait for last played to update", () => getLastPlayed() != null);
        }

        [Test]
        public void TestScoreStoredLocally()
        {
            CreateTest();

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);

            AddStep("seek to completion", () => Player.GameplayClockContainer.Seek(Player.DrawableRuleset.Objects.Last().GetEndTime()));

            AddUntilStep("results displayed", () => Player.GetChildScreen() is ResultsScreen);
            AddUntilStep("score in database", () => Realm.Run(r => r.Find<ScoreInfo>(Player.Score.ScoreInfo.ID) != null));
        }

        [Test]
        public void TestScoreStoredLocallyCustomRuleset()
        {
            Ruleset createCustomRuleset() => new CustomRuleset();

            AddStep("import custom ruleset", () => Realm.Write(r => r.Add(createCustomRuleset().RulesetInfo)));
            AddStep("set custom ruleset", () => customRuleset = createCustomRuleset());

            CreateTest();

            AddAssert("score has custom ruleset", () => Player.Score.ScoreInfo.Ruleset.Equals(customRuleset.AsNonNull().RulesetInfo));

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);

            AddStep("seek to completion", () => Player.GameplayClockContainer.Seek(Player.DrawableRuleset.Objects.Last().GetEndTime()));

            AddUntilStep("results displayed", () => Player.GetChildScreen() is ResultsScreen);
            AddUntilStep("score in database", () => Realm.Run(r => r.Find<ScoreInfo>(Player.Score.ScoreInfo.ID) != null));
        }

        private class CustomRuleset : OsuRuleset, ILegacyRuleset
        {
            public override string Description => "custom";
            public override string ShortName => "custom";

            int ILegacyRuleset.LegacyID => -1;

            public override ScoreProcessor CreateScoreProcessor() => new ScoreProcessor(this);
        }
    }
}
