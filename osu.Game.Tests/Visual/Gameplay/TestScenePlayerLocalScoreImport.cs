// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Tests.Visual.Gameplay
{
    [HeadlessTest] // Importing rulesets doesn't work in interactive flows.
    public class TestScenePlayerLocalScoreImport : PlayerTestScene
    {
        private Ruleset? customRuleset;

        protected override bool ImportBeatmapToDatabase => true;

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
            Ruleset createCustomRuleset() => new OsuRuleset
            {
                RulesetInfo =
                {
                    Name = "custom",
                    ShortName = "custom",
                    OnlineID = -1
                }
            };

            AddStep("import custom ruleset", () => Realm.Write(r => r.Add(createCustomRuleset().RulesetInfo)));
            AddStep("set custom ruleset", () => customRuleset = createCustomRuleset());

            CreateTest();

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);

            AddStep("seek to completion", () => Player.GameplayClockContainer.Seek(Player.DrawableRuleset.Objects.Last().GetEndTime()));

            AddUntilStep("results displayed", () => Player.GetChildScreen() is ResultsScreen);
            AddUntilStep("score in database", () => Realm.Run(r => r.All<ScoreInfo>().Count() == 1));
        }
    }
}
