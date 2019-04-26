// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Screens.Select;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelect
{
    [TestFixture]
    public class TestCaseBeatmapPlaylist : ManualInputManagerTestCase
    {
        private RulesetStore rulesets;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(BeatmapPlaylistItem),
            typeof(BeatmapPlaylist),
        };

        private int lastInsert;
        private TestBeatmapPlaylist playlist;

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            this.rulesets = rulesets;
            Add(playlist = new TestBeatmapPlaylist());
        }

        [SetUp]
        public void SetUp()
        {
            lastInsert = 0;
            playlist.Clear();
            for (int i = 0; i < 4; i++)
                playlist.AddItem(generatePlaylistItem(rulesets.GetRuleset(lastInsert++ % 4)));
        }

        [Test]
        public void AddRemoveTests()
        {
            AddStep("Hover Remove Button", () => { InputManager.MoveMouseTo(playlist.GetChild(0).ToScreenSpace(playlist.GetChildSize(0) + new Vector2(-20, -playlist.GetChildSize(0).Y * 0.5f))); });
            AddStep("RemoveItem", () => InputManager.Click(MouseButton.Left));
            AddAssert("Ensure correct child count", () => playlist.Count == 3);
            AddStep("AddItem", () => { playlist.AddItem(generatePlaylistItem(rulesets.GetRuleset(lastInsert++ % 4))); });
            AddAssert("Ensure correct child count", () => playlist.Count == 4);
        }

        [Test]
        public void SortingTests()
        {
            AddStep("Hover drag handle", () => { InputManager.MoveMouseTo(playlist.GetChild(0).ToScreenSpace(new Vector2(10, playlist.GetChildSize(0).Y * 0.5f))); });
            AddStep("Click", () => { InputManager.PressButton(MouseButton.Left); });
            AddStep("Drag downward", () => { InputManager.MoveMouseTo(playlist.GetChild(0).ToScreenSpace(new Vector2(10, playlist.GetChildSize(0).Y * 2.5f))); });
            AddStep("Release", () => { InputManager.ReleaseButton(MouseButton.Left); });
            AddAssert("Ensure item is now third", () => playlist.GetLayoutPosition(playlist.GetChild(0)) == 2);

            AddStep("Hover drag handle", () => { InputManager.MoveMouseTo(playlist.GetChild(0).ToScreenSpace(new Vector2(10, playlist.GetChildSize(0).Y * 0.5f))); });
            AddStep("Click", () => { InputManager.PressButton(MouseButton.Left); });
            AddStep("Drag upward", () => { InputManager.MoveMouseTo(playlist.GetChild(0).ToScreenSpace(new Vector2(10, -playlist.GetChildSize(0).Y * 1.5f))); });
            AddStep("Release", () => { InputManager.ReleaseButton(MouseButton.Left); });
            AddAssert("Ensure item is now first again", () => playlist.GetLayoutPosition(playlist.GetChild(0)) == 0);
        }

        private BeatmapPlaylistItem generatePlaylistItem(RulesetInfo ruleset)
        {
            var beatmap = new TestBeatmap(ruleset);
            var playlistItem = new PlaylistItem
            {
                Beatmap = beatmap.BeatmapInfo,
                Ruleset = beatmap.BeatmapInfo.Ruleset,
                RulesetID = beatmap.BeatmapInfo.Ruleset?.ID ?? 0
            };

            // TODO: Use more realistic mod combinations here
            var instance = ruleset.CreateInstance();
            playlistItem.RequiredMods.AddRange(instance.GetAllMods());

            return new BeatmapPlaylistItem(playlistItem);
        }

        private class TestBeatmapPlaylist : BeatmapPlaylist
        {
            public IReadOnlyList<Drawable> Children => ListContainer.Children;

            public float GetLayoutPosition(Drawable d) => ListContainer.GetLayoutPosition(d);

            public Drawable GetChild(int index) => Children[index];

            public Vector2 GetChildSize(int index) => GetChild(index).DrawSize;

            public int Count => ListContainer.Count;
        }
    }
}
