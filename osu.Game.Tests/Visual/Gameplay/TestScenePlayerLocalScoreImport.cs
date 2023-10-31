// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
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
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Tests.Resources;
using osu.Game.Users;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestScenePlayerLocalScoreImport : PlayerTestScene
    {
        private BeatmapManager beatmaps = null!;
        private RulesetStore rulesets = null!;

        private BeatmapSetInfo? importedSet;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(new ScoreManager(rulesets, () => beatmaps, LocalStorage, Realm, API));
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
            AddUntilStep("wait for button clickable", () => Player.ChildrenOfType<SaveFailedScoreButton>().First().ChildrenOfType<OsuClickableContainer>().First().Enabled.Value);

            AddUntilStep("score not in database", () => Realm.Run(r => r.Find<ScoreInfo>(Player.Score.ScoreInfo.ID) == null));
            AddStep("click save button", () => Player.ChildrenOfType<SaveFailedScoreButton>().First().ChildrenOfType<OsuClickableContainer>().First().TriggerClick());
            AddUntilStep("score in database", () => Realm.Run(r => r.Find<ScoreInfo>(Player.Score.ScoreInfo.ID) != null));
        }

        [Test]
        public void TestLastPlayedUpdated()
        {
            DateTimeOffset? getLastPlayed() => Realm.Run(r => r.Find<BeatmapInfo>(Beatmap.Value.BeatmapInfo.ID)?.LastPlayed);

            AddStep("reset last played", () => Realm.Write(r => r.Find<BeatmapInfo>(Beatmap.Value.BeatmapInfo.ID)!.LastPlayed = null));
            AddAssert("last played is null", () => getLastPlayed() == null);

            CreateTest();

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);
            AddUntilStep("wait for last played to update", () => getLastPlayed() != null);
        }

        [Test]
        public void TestModReferenceNotRetained()
        {
            AddStep("allow fail", () => allowFail = false);

            Mod[] originalMods = { new OsuModDaycore { SpeedChange = { Value = 0.8 } } };
            Mod[] playerMods = null!;

            AddStep("load player with mods", () => LoadPlayer(originalMods));
            AddUntilStep("player loaded", () => Player.IsLoaded && Player.Alpha == 1);

            AddStep("get mods at start of gameplay", () => playerMods = Player.Score.ScoreInfo.Mods.ToArray());

            // Player creates new instance of mods during load.
            AddAssert("player score has copied mods", () => playerMods.First(), () => Is.Not.SameAs(originalMods.First()));
            AddAssert("player score has matching mods", () => playerMods.First(), () => Is.EqualTo(originalMods.First()));

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);

            AddStep("seek to completion", () => Player.GameplayClockContainer.Seek(Player.DrawableRuleset.Objects.Last().GetEndTime()));

            AddUntilStep("results displayed", () => Player.GetChildScreen() is ResultsScreen);

            // Player creates new instance of mods after gameplay to ensure any runtime references to drawables etc. are not retained.
            AddAssert("results screen score has copied mods", () => (Player.GetChildScreen() as ResultsScreen)?.Score.Mods.First(), () => Is.Not.SameAs(playerMods.First()));
            AddAssert("results screen score has matching", () => (Player.GetChildScreen() as ResultsScreen)?.Score.Mods.First(), () => Is.EqualTo(playerMods.First()));

            AddUntilStep("score in database", () => Realm.Run(r => r.Find<ScoreInfo>(Player.Score.ScoreInfo.ID) != null));
            AddUntilStep("databased score has correct mods", () => Realm.Run(r => r.Find<ScoreInfo>(Player.Score.ScoreInfo.ID))!.Mods.First(), () => Is.EqualTo(playerMods.First()));
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
        public void TestGuestScoreIsStoredAsGuest()
        {
            AddStep("set up API", () => ((DummyAPIAccess)API).HandleRequest = req =>
            {
                switch (req)
                {
                    case GetUserRequest userRequest:
                        userRequest.TriggerSuccess(new APIUser
                        {
                            Username = "Guest",
                            CountryCode = CountryCode.JP,
                            Id = 1234
                        });
                        return true;

                    default:
                        return false;
                }
            });

            AddStep("log out", () => API.Logout());
            CreateTest();

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);
            AddStep("log back in", () => API.Login("username", "password"));

            AddStep("seek to completion", () => Player.GameplayClockContainer.Seek(Player.DrawableRuleset.Objects.Last().GetEndTime()));

            AddUntilStep("results displayed", () => Player.GetChildScreen() is ResultsScreen);
            AddUntilStep("score in database", () => Realm.Run(r => r.Find<ScoreInfo>(Player.Score.ScoreInfo.ID) != null));
            AddAssert("score is not associated with online user", () => Realm.Run(r => r.Find<ScoreInfo>(Player.Score.ScoreInfo.ID))!.UserID == APIUser.SYSTEM_USER_ID);
        }

        [Test]
        public void TestReplayExport()
        {
            CreateTest();

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);

            AddStep("seek to completion", () => Player.GameplayClockContainer.Seek(Player.DrawableRuleset.Objects.Last().GetEndTime()));

            AddUntilStep("results displayed", () => (Player.GetChildScreen() as ResultsScreen)?.IsLoaded == true);
            AddUntilStep("score in database", () => Realm.Run(r => r.Find<ScoreInfo>(Player.Score.ScoreInfo.ID) != null));

            AddUntilStep("wait for button clickable", () => ((OsuScreen)Player.GetChildScreen())
                                                            .ChildrenOfType<ReplayDownloadButton>().FirstOrDefault()?
                                                            .ChildrenOfType<OsuClickableContainer>().FirstOrDefault()?
                                                            .Enabled.Value == true);

            AddAssert("no export files", () => !LocalStorage.GetFiles("exports").Any());

            AddStep("Export replay", () => InputManager.PressKey(Key.F2));

            string? filePath = null;

            // Files starting with _ are temporary, created by CreateFileSafely call.
            AddUntilStep("wait for export file", () => filePath = LocalStorage.GetFiles("exports").SingleOrDefault(f => !Path.GetFileName(f).StartsWith("_", StringComparison.Ordinal)), () => Is.Not.Null);
            AddUntilStep("filesize is non-zero", () =>
            {
                try
                {
                    using (var stream = LocalStorage.GetStream(filePath))
                        return stream.Length;
                }
                catch (IOException)
                {
                    // file move may still be in progress.
                    return 0;
                }
            }, () => Is.Not.Zero);
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
