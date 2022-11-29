// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
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
using osu.Game.Screens.Select;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneMultiplayerMatchSongSelect : MultiplayerTestScene
    {
        private BeatmapManager manager;
        private RulesetStore rulesets;

        private IList<BeatmapInfo> beatmaps => importedBeatmapSet?.PerformRead(s => s.Beatmaps) ?? new List<BeatmapInfo>();

        private TestMultiplayerMatchSongSelect songSelect;

        private Live<BeatmapSetInfo> importedBeatmapSet;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, Realm, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);

            importedBeatmapSet = manager.Import(TestResources.CreateTestBeatmapSetInfo(8, rulesets.AvailableRulesets.ToArray()));
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("reset", () =>
            {
                Ruleset.Value = new OsuRuleset().RulesetInfo;
                Beatmap.SetDefault();
                SelectedMods.SetDefault();
            });

            AddStep("create song select", () => LoadScreen(songSelect = new TestMultiplayerMatchSongSelect(SelectedRoom.Value)));
            AddUntilStep("wait for present", () => songSelect.IsCurrentScreen() && songSelect.BeatmapSetsLoaded);
        }

        [Test]
        public void TestBeatmapConfirmed()
        {
            BeatmapInfo selectedBeatmap = null;

            AddStep("change ruleset", () => Ruleset.Value = new TaikoRuleset().RulesetInfo);
            AddStep("select beatmap",
                () => songSelect.Carousel.SelectBeatmap(selectedBeatmap = beatmaps.First(beatmap => beatmap.Ruleset.OnlineID == new TaikoRuleset().LegacyID)));

            AddUntilStep("wait for selection", () => Beatmap.Value.BeatmapInfo.Equals(selectedBeatmap));
            AddUntilStep("wait for ongoing operation to complete", () => !OnlinePlayDependencies.OngoingOperationTracker.InProgress.Value);

            AddStep("set mods", () => SelectedMods.Value = new[] { new TaikoModDoubleTime() });

            AddStep("confirm selection", () => songSelect.FinaliseSelection());

            AddUntilStep("song select exited", () => !songSelect.IsCurrentScreen());

            AddAssert("beatmap not changed", () => Beatmap.Value.BeatmapInfo.Equals(selectedBeatmap));
            AddAssert("ruleset not changed", () => Ruleset.Value.Equals(new TaikoRuleset().RulesetInfo));
            AddAssert("mods not changed", () => SelectedMods.Value.Single() is TaikoModDoubleTime);
        }

        [TestCase(typeof(OsuModHidden), typeof(OsuModHidden))] // Same mod.
        [TestCase(typeof(OsuModHidden), typeof(OsuModTraceable))] // Incompatible.
        public void TestAllowedModDeselectedWhenRequired(Type allowedMod, Type requiredMod)
        {
            AddStep($"select {allowedMod.ReadableName()} as allowed", () => songSelect.FreeMods.Value = new[] { (Mod)Activator.CreateInstance(allowedMod) });
            AddStep($"select {requiredMod.ReadableName()} as required", () => songSelect.Mods.Value = new[] { (Mod)Activator.CreateInstance(requiredMod) });

            AddAssert("freemods empty", () => songSelect.FreeMods.Value.Count == 0);
            assertHasFreeModButton(allowedMod, false);
            assertHasFreeModButton(requiredMod, false);
        }

        private void assertHasFreeModButton(Type type, bool hasButton = true)
        {
            AddAssert($"{type.ReadableName()} {(hasButton ? "displayed" : "not displayed")} in freemod overlay",
                () => this.ChildrenOfType<FreeModSelectOverlay>()
                          .Single()
                          .ChildrenOfType<ModPanel>()
                          .Where(panel => !panel.Filtered.Value)
                          .All(b => b.Mod.GetType() != type));
        }

        private partial class TestMultiplayerMatchSongSelect : MultiplayerMatchSongSelect
        {
            public new Bindable<IReadOnlyList<Mod>> Mods => base.Mods;

            public new Bindable<IReadOnlyList<Mod>> FreeMods => base.FreeMods;

            public new BeatmapCarousel Carousel => base.Carousel;

            public TestMultiplayerMatchSongSelect(Room room)
                : base(room)
            {
            }
        }
    }
}
