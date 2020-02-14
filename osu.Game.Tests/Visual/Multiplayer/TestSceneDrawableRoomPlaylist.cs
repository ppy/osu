// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Multi;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneDrawableRoomPlaylist : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DrawableRoomPlaylist),
            typeof(DrawableRoomPlaylistItem)
        };

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        [Resolved]
        private RulesetStore rulesetStore { get; set; }

        private DrawableRoomPlaylist playlist;

        [Test]
        public void TestNonEditableNonSelectable()
        {
            createPlaylist(false, false);
        }

        [Test]
        public void TestEditable()
        {
            createPlaylist(true, false);
        }

        [Test]
        public void TestSelectable()
        {
            createPlaylist(false, true);
        }

        [Test]
        public void TestEditableSelectable()
        {
            createPlaylist(true, true);
        }

        private void createPlaylist(bool allowEdit, bool allowSelection) => AddStep("create playlist", () =>
        {
            Child = playlist = new DrawableRoomPlaylist(allowEdit, allowSelection)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(500, 300)
            };

            var beatmapSets = beatmapManager.GetAllUsableBeatmapSets();
            var rulesets = rulesetStore.AvailableRulesets.ToList();

            for (int i = 0; i < 20; i++)
            {
                var set = beatmapSets[RNG.Next(0, beatmapSets.Count)];
                var beatmap = set.Beatmaps[RNG.Next(0, set.Beatmaps.Count)];

                beatmap.BeatmapSet = set;
                beatmap.Metadata = set.Metadata;

                playlist.Items.Add(new PlaylistItem
                {
                    Beatmap = { Value = beatmap },
                    Ruleset = { Value = rulesets[RNG.Next(0, rulesets.Count)] },
                    RequiredMods =
                    {
                        new OsuModHardRock(),
                        new OsuModDoubleTime(),
                        new OsuModAutoplay()
                    }
                });
            }
        });
    }
}
