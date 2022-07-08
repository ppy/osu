// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
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
            Dependencies.Cache(new ScoreManager(rulesets, () => beatmaps, LocalStorage, Realm, Scheduler));
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

        protected override bool AllowFail => false;

        [Test]
        public void TestScoreStoredLocally()
        {
            AddStep("set no custom ruleset", () => customRuleset = null);

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

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);

            AddStep("seek to completion", () => Player.GameplayClockContainer.Seek(Player.DrawableRuleset.Objects.Last().GetEndTime()));

            AddUntilStep("results displayed", () => Player.GetChildScreen() is ResultsScreen);
            AddUntilStep("score in database", () => Realm.Run(r => r.Find<ScoreInfo>(Player.Score.ScoreInfo.ID) != null));
        }

        private class CustomRuleset : OsuRuleset, ILegacyRuleset
        {
            public override string Description => "custom";
            public override string ShortName => "custom";

            public new int LegacyID => -1;

            public override ScoreProcessor CreateScoreProcessor() => new ScoreProcessor(this);
        }
    }
}
