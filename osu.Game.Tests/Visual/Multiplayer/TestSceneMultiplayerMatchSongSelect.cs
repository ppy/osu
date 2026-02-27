// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Tests.Resources;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneMultiplayerMatchSongSelect : MultiplayerTestScene
    {
        private BeatmapManager manager = null!;
        private RulesetStore rulesets = null!;

        private IList<BeatmapInfo> beatmaps => importedBeatmapSet.PerformRead(s => s.Beatmaps);

        private TestMultiplayerMatchSongSelect songSelect = null!;
        private Live<BeatmapSetInfo> importedBeatmapSet = null!;
        private Room room = null!;

        [Resolved]
        private OsuConfigManager configManager { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            BeatmapStore beatmapStore;

            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, Realm, null, audio, Resources, host, Beatmap.Default));
            Dependencies.CacheAs(beatmapStore = new RealmDetachedBeatmapStore());
            Dependencies.Cache(Realm);

            importedBeatmapSet = manager.Import(TestResources.CreateTestBeatmapSetInfo(8, rulesets.AvailableRulesets.ToArray()))!;

            Add(beatmapStore);
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("create room", () => room = CreateDefaultRoom());
            AddStep("join room", () => JoinRoom(room));
            WaitForJoined();
        }

        private void setUp()
        {
            AddStep("create song select", () =>
            {
                Ruleset.Value = new OsuRuleset().RulesetInfo;
                Beatmap.SetDefault();
                SelectedMods.SetDefault();

                LoadScreen(songSelect = new TestMultiplayerMatchSongSelect(room));
            });

            AddUntilStep("wait for present", () => songSelect.IsCurrentScreen() && !songSelect.IsFiltering);
        }

        [Test]
        public void TestSelectFreeMods()
        {
            setUp();

            AddStep("set some freemods", () => songSelect.FreeMods.Value = new OsuRuleset().GetModsFor(ModType.Fun).ToArray());
            AddStep("set all freemods", () => songSelect.FreeMods.Value = new OsuRuleset().CreateAllMods().ToArray());
            AddStep("set no freemods", () => songSelect.FreeMods.Value = Array.Empty<Mod>());
        }

        [Test]
        public void TestBeatmapConfirmed()
        {
            BeatmapInfo selectedBeatmap = null!;

            setUp();

            AddStep("change ruleset", () => Ruleset.Value = new TaikoRuleset().RulesetInfo);

            AddUntilStep("wait for filtering", () => !songSelect.IsFiltering);
            AddStep("select beatmap",
                () => songSelect.SelectBeatmap(selectedBeatmap = beatmaps.First(beatmap => beatmap.Ruleset.OnlineID == new TaikoRuleset().LegacyID)));

            AddUntilStep("wait for selection", () => Beatmap.Value.BeatmapInfo.Equals(selectedBeatmap));
            AddUntilStep("wait for ongoing operation to complete", () => !OnlinePlayDependencies.OngoingOperationTracker.InProgress.Value);

            AddStep("set mods", () => SelectedMods.Value = new[] { new TaikoModDoubleTime() });

            AddStep("confirm selection", () => InputManager.Key(Key.Enter));

            AddUntilStep("song select exited", () => !songSelect.IsCurrentScreen());

            AddAssert("beatmap not changed", () => Beatmap.Value.BeatmapInfo, () => Is.EqualTo((selectedBeatmap)));
            AddAssert("ruleset not changed", () => Ruleset.Value.Equals(new TaikoRuleset().RulesetInfo));
            AddAssert("mods not changed", () => SelectedMods.Value.Single() is TaikoModDoubleTime);
        }

        [TestCase(typeof(OsuModHidden), typeof(OsuModHidden))] // Same mod.
        [TestCase(typeof(OsuModHidden), typeof(OsuModTraceable))] // Incompatible.
        public void TestAllowedModDeselectedWhenRequired(Type allowedMod, Type requiredMod)
        {
            setUp();

            AddStep("change ruleset", () => Ruleset.Value = new OsuRuleset().RulesetInfo);
            AddStep($"select {allowedMod.ReadableName()} as allowed", () => songSelect.FreeMods.Value = new[] { (Mod)Activator.CreateInstance(allowedMod)! });
            AddStep($"select {requiredMod.ReadableName()} as required", () => songSelect.Mods.Value = new[] { (Mod)Activator.CreateInstance(requiredMod)! });

            AddAssert("freemods empty", () => songSelect.FreeMods.Value.Count == 0);

            // A previous test's mod overlay could still be fading out.
            AddUntilStep("wait for only one freemod overlay", () => this.ChildrenOfType<FreeModSelectOverlay>().Count() == 1);

            AddStep("open free mod overlay", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<FooterButtonFreeMods>().Single());
                InputManager.Click(MouseButton.Left);
            });

            assertFreeModNotShown(allowedMod);
            assertFreeModNotShown(requiredMod);
        }

        [Test]
        public void TestFreeModsDisplayedOnEnter()
        {
            AddStep("set room freemods", () =>
            {
                var editedItem = MultiplayerClient.ClientRoom!.CurrentPlaylistItem.Clone();

                editedItem.AllowedMods =
                [
                    new APIMod(new OsuModHardRock()),
                ];

                MultiplayerClient.EditPlaylistItem(editedItem);
            });

            setUp();

            AddStep("open free mod overlay", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<FooterButtonFreeMods>().Single());
                InputManager.Click(MouseButton.Left);
            });

            assertFreeModShown(typeof(OsuModHardRock));
        }

        [Test]
        public void TestChangeRulesetImmediatelyAfterLoadComplete()
        {
            AddStep("reset", () =>
            {
                configManager.SetValue(OsuSetting.ShowConvertedBeatmaps, false);
                Beatmap.SetDefault();
                SelectedMods.SetDefault();
            });

            AddStep("create song select", () =>
            {
                room.Playlist.Single().RulesetID = 2;
                songSelect = new TestMultiplayerMatchSongSelect(room, room.Playlist.Single());
                songSelect.OnLoadComplete += _ => Ruleset.Value = new TaikoRuleset().RulesetInfo;
                LoadScreen(songSelect);
            });

            AddUntilStep("wait for present", () => songSelect.IsCurrentScreen() && !songSelect.IsFiltering);

            AddStep("confirm selection", () => InputManager.Key(Key.Enter));
            AddAssert("beatmap is taiko", () => Beatmap.Value.BeatmapInfo.Ruleset.OnlineID, () => Is.EqualTo(1));
            AddAssert("ruleset is taiko", () => Ruleset.Value.OnlineID, () => Is.EqualTo(1));
        }

        private void assertFreeModShown(Type type)
        {
            AddUntilStep($"{type.ReadableName()} displayed in freemod overlay",
                () => this.ChildrenOfType<FreeModSelectOverlay>()
                          .Single()
                          .ChildrenOfType<ModPanel>()
                          .Where(panel => panel.Visible)
                          .Any(b => b.Mod.GetType() == type));
        }

        private void assertFreeModNotShown(Type type)
        {
            AddUntilStep($"{type.ReadableName()} not displayed in freemod overlay",
                () => this.ChildrenOfType<FreeModSelectOverlay>()
                          .Single()
                          .ChildrenOfType<ModPanel>()
                          .Where(panel => panel.Visible)
                          .All(b => b.Mod.GetType() != type));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (rulesets.IsNotNull())
                rulesets.Dispose();
        }

        private partial class TestMultiplayerMatchSongSelect : MultiplayerMatchSongSelect
        {
            public new Bindable<IReadOnlyList<Mod>> Mods => base.Mods;

            public new Bindable<IReadOnlyList<Mod>> FreeMods => base.FreeMods;

            public TestMultiplayerMatchSongSelect(Room room, PlaylistItem? itemToEdit = null)
                : base(room, itemToEdit)
            {
            }

            public void SelectBeatmap(BeatmapInfo beatmap) => SelectAndRun(beatmap, () => { });
        }
    }
}
