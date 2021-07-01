// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Online.Solo;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Ranking;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestScenePlayerScoreSubmission : OsuPlayerTestScene
    {
        protected override bool AllowFail => false;

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        protected override TestPlayer CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = new[] { ruleset.GetAllMods().OfType<ModNoFail>().First() };
            return new TestPlayer(false);
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            AddStep("Prepare test API", () =>
            {
                dummyAPI.HandleRequest = request =>
                {
                    switch (request)
                    {
                        case CreateSoloScoreRequest tokenRequest:
                            tokenRequest.TriggerSuccess(new APIScoreToken { ID = 1234 });
                            return true;
                    }

                    return false;
                };
            });

            base.SetUpSteps();

            // Ensure track has actually running before attempting to seek
            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);
        }

        [Test]
        public void TestSubmissionOnResults()
        {
            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);

            AddStep("seek to completion", () => Player.GameplayClockContainer.Seek(Player.DrawableRuleset.Objects.Last().GetEndTime()));

            AddUntilStep("results displayed", () => Player.GetChildScreen() is ResultsScreen);

            AddUntilStep("wait for submission", () => Player.SubmissionRequested);
        }

        [Test]
        public void TestSubmissionOnExit()
        {
            AddUntilStep("wait for token request", () => Player.TokenCreationRequested);
            AddStep("exit", () => Player.Exit());
            AddUntilStep("wait for submission", () => Player.SubmissionRequested);
        }
    }
}
