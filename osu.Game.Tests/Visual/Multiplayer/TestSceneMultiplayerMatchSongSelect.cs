// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Online.Rooms;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerMatchSongSelect : MultiplayerTestScene
    {
        private BeatmapManager manager;
        private RulesetStore rulesets;

        private List<BeatmapInfo> beatmaps;

        private TestMultiplayerMatchSongSelect songSelect;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, Resources, host, Beatmap.Default));

            beatmaps = new List<BeatmapInfo>();

            var metadata = new BeatmapMetadata
            {
                Artist = "Some Artist",
                Title = "Some Beatmap",
                AuthorString = "Some Author"
            };

            var beatmapSetInfo = new BeatmapSetInfo
            {
                OnlineID = 10,
                Hash = Guid.NewGuid().ToString().ComputeMD5Hash(),
                Metadata = metadata,
                DateAdded = DateTimeOffset.UtcNow
            };

            for (int i = 0; i < 8; ++i)
            {
                int beatmapId = 10 * 10 + i;

                int length = RNG.Next(30000, 200000);
                double bpm = RNG.NextSingle(80, 200);

                var beatmap = new BeatmapInfo
                {
                    Ruleset = rulesets.GetRuleset(i % 4),
                    OnlineID = beatmapId,
                    Length = length,
                    BPM = bpm,
                    Metadata = metadata,
                    BaseDifficulty = new BeatmapDifficulty()
                };

                beatmaps.Add(beatmap);
                beatmapSetInfo.Beatmaps.Add(beatmap);
            }

            manager.Import(beatmapSetInfo).Wait();
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
        public void TestBeatmapRevertedOnExitIfNoSelection()
        {
            BeatmapInfo selectedBeatmap = null;

            AddStep("select beatmap",
                () => songSelect.Carousel.SelectBeatmap(selectedBeatmap = beatmaps.Where(beatmap => beatmap.RulesetID == new OsuRuleset().LegacyID).ElementAt(1)));
            AddUntilStep("wait for selection", () => Beatmap.Value.BeatmapInfo.Equals(selectedBeatmap));

            AddStep("exit song select", () => songSelect.Exit());
            AddAssert("beatmap reverted", () => Beatmap.IsDefault);
        }

        [Test]
        public void TestModsRevertedOnExitIfNoSelection()
        {
            AddStep("change mods", () => SelectedMods.Value = new[] { new OsuModDoubleTime() });

            AddStep("exit song select", () => songSelect.Exit());
            AddAssert("mods reverted", () => SelectedMods.Value.Count == 0);
        }

        [Test]
        public void TestRulesetRevertedOnExitIfNoSelection()
        {
            AddStep("change ruleset", () => Ruleset.Value = new CatchRuleset().RulesetInfo);

            AddStep("exit song select", () => songSelect.Exit());
            AddAssert("ruleset reverted", () => Ruleset.Value.Equals(new OsuRuleset().RulesetInfo));
        }

        [Test]
        public void TestBeatmapConfirmed()
        {
            BeatmapInfo selectedBeatmap = null;

            AddStep("change ruleset", () => Ruleset.Value = new TaikoRuleset().RulesetInfo);
            AddStep("select beatmap",
                () => songSelect.Carousel.SelectBeatmap(selectedBeatmap = beatmaps.First(beatmap => beatmap.RulesetID == new TaikoRuleset().LegacyID)));
            AddUntilStep("wait for selection", () => Beatmap.Value.BeatmapInfo.Equals(selectedBeatmap));
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
                () => songSelect.ChildrenOfType<FreeModSelectOverlay>().Single().ChildrenOfType<ModButton>().All(b => b.Mod.GetType() != type));
        }

        private class TestMultiplayerMatchSongSelect : MultiplayerMatchSongSelect
        {
            public new Bindable<IReadOnlyList<Mod>> Mods => base.Mods;

            public new Bindable<IReadOnlyList<Mod>> FreeMods => base.FreeMods;

            public new BeatmapCarousel Carousel => base.Carousel;

            public TestMultiplayerMatchSongSelect(Room room, WorkingBeatmap beatmap = null, RulesetInfo ruleset = null)
                : base(room, null, beatmap, ruleset)
            {
            }
        }
    }
}
