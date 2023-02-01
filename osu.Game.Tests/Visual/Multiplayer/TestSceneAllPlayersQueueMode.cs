// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Screens.Play;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneAllPlayersQueueMode : QueueModeTestScene
    {
        protected override QueueMode Mode => QueueMode.AllPlayers;

        [Test]
        public void TestFirstItemSelectedByDefault()
        {
            AddUntilStep("first item selected", () => MultiplayerClient.ClientRoom?.Settings.PlaylistItemId == MultiplayerClient.ClientAPIRoom?.Playlist[0].ID);
        }

        [Test]
        public void TestSingleItemExpiredAfterGameplay()
        {
            RunGameplay();

            AddUntilStep("playlist has only one item", () => MultiplayerClient.ClientAPIRoom?.Playlist.Count == 1);
            AddUntilStep("playlist item is expired", () => MultiplayerClient.ClientAPIRoom?.Playlist[0].Expired == true);
            AddUntilStep("last item selected", () => MultiplayerClient.ClientRoom?.Settings.PlaylistItemId == MultiplayerClient.ClientAPIRoom?.Playlist[0].ID);
        }

        [Test]
        public void TestItemAddedToTheEndOfQueue()
        {
            addItem(() => OtherBeatmap);
            AddUntilStep("playlist has 2 items", () => MultiplayerClient.ClientAPIRoom?.Playlist.Count == 2);

            addItem(() => InitialBeatmap);
            AddUntilStep("playlist has 3 items", () => MultiplayerClient.ClientAPIRoom?.Playlist.Count == 3);

            AddUntilStep("first item still selected", () => MultiplayerClient.ClientRoom?.Settings.PlaylistItemId == MultiplayerClient.ClientAPIRoom?.Playlist[0].ID);
        }

        [Test]
        public void TestNextItemSelectedAfterGameplayFinish()
        {
            addItem(() => OtherBeatmap);
            addItem(() => InitialBeatmap);

            RunGameplay();

            AddUntilStep("first item expired", () => MultiplayerClient.ClientAPIRoom?.Playlist[0].Expired == true);
            AddUntilStep("next item selected", () => MultiplayerClient.ClientRoom?.Settings.PlaylistItemId == MultiplayerClient.ClientAPIRoom?.Playlist[1].ID);

            RunGameplay();

            AddUntilStep("second item expired", () => MultiplayerClient.ClientAPIRoom?.Playlist[1].Expired == true);
            AddUntilStep("next item selected", () => MultiplayerClient.ClientRoom?.Settings.PlaylistItemId == MultiplayerClient.ClientAPIRoom?.Playlist[2].ID);
        }

        [Test]
        public void TestItemsNotClearedWhenSwitchToHostOnlyMode()
        {
            addItem(() => OtherBeatmap);
            addItem(() => InitialBeatmap);

            // Move to the "other" beatmap.
            RunGameplay();

            AddStep("change queue mode", () => MultiplayerClient.ChangeSettings(queueMode: QueueMode.HostOnly));
            AddUntilStep("playlist has 3 items", () => MultiplayerClient.ClientAPIRoom?.Playlist.Count == 3);
            AddUntilStep("item 2 is not expired", () => MultiplayerClient.ClientAPIRoom?.Playlist[1].Expired == false);
            AddUntilStep("current item is the other beatmap", () => MultiplayerClient.ClientRoom?.Settings.PlaylistItemId == 2);
        }

        [Test]
        public void TestCorrectItemSelectedAfterNewItemAdded()
        {
            addItem(() => OtherBeatmap);
            AddUntilStep("selected beatmap is initial beatmap", () => Beatmap.Value.BeatmapInfo.OnlineID == InitialBeatmap.OnlineID);
        }

        [Test]
        public void TestCorrectRulesetSelectedAfterNewItemAdded()
        {
            addItem(() => OtherBeatmap, new CatchRuleset().RulesetInfo);
            AddUntilStep("selected beatmap is initial beatmap", () => Beatmap.Value.BeatmapInfo.OnlineID == InitialBeatmap.OnlineID);

            AddUntilStep("wait for idle", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Idle);
            ClickButtonWhenEnabled<MultiplayerReadyButton>();

            AddUntilStep("wait for ready", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Ready);
            ClickButtonWhenEnabled<MultiplayerReadyButton>();

            AddUntilStep("wait for player", () => CurrentScreen is Player player && player.IsLoaded);
            AddAssert("ruleset is correct", () => ((Player)CurrentScreen).Ruleset.Value.Equals(new OsuRuleset().RulesetInfo));
            AddStep("exit player", () => CurrentScreen.Exit());
        }

        [Test]
        public void TestCorrectModsSelectedAfterNewItemAdded()
        {
            addItem(() => OtherBeatmap, mods: new Mod[] { new OsuModDoubleTime() });
            AddUntilStep("selected beatmap is initial beatmap", () => Beatmap.Value.BeatmapInfo.OnlineID == InitialBeatmap.OnlineID);

            AddUntilStep("wait for idle", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Idle);
            ClickButtonWhenEnabled<MultiplayerReadyButton>();

            AddUntilStep("wait for ready", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Ready);
            ClickButtonWhenEnabled<MultiplayerReadyButton>();

            AddUntilStep("wait for player", () => CurrentScreen is Player player && player.IsLoaded);
            AddAssert("mods are correct", () => !((Player)CurrentScreen).Mods.Value.Any());
            AddStep("exit player", () => CurrentScreen.Exit());
        }

        private void addItem(Func<BeatmapInfo> beatmap, RulesetInfo? ruleset = null, IReadOnlyList<Mod>? mods = null)
        {
            Screens.Select.SongSelect? songSelect = null;

            AddStep("click add button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerMatchSubScreen.AddItemButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for song select", () => (songSelect = CurrentSubScreen as Screens.Select.SongSelect) != null);
            AddUntilStep("wait for loaded", () => songSelect.AsNonNull().BeatmapSetsLoaded);

            if (ruleset != null)
                AddStep($"set {ruleset.Name} ruleset", () => songSelect.AsNonNull().Ruleset.Value = ruleset);

            if (mods != null)
                AddStep($"set mods to {string.Join(",", mods.Select(m => m.Acronym))}", () => songSelect.AsNonNull().Mods.Value = mods);

            AddStep("select other beatmap", () => songSelect.AsNonNull().FinaliseSelection(beatmap()));
            AddUntilStep("wait for return to match", () => CurrentSubScreen is MultiplayerMatchSubScreen);
        }
    }
}
