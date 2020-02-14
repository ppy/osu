// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Multi.Components;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMatchBeatmapDetailArea : MultiplayerTestScene
    {
        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        [Resolved]

        private RulesetStore rulesetStore { get; set; }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Room.Playlist.Clear();

            Child = new MatchBeatmapDetailArea
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(500),
                CreateNewItem = createNewItem
            };
        });

        private void createNewItem()
        {
            var set = beatmapManager.GetAllUsableBeatmapSetsEnumerable().First();
            var rulesets = rulesetStore.AvailableRulesets.ToList();

            var beatmap = set.Beatmaps[RNG.Next(0, set.Beatmaps.Count)];

            beatmap.BeatmapSet = set;
            beatmap.Metadata = set.Metadata;

            Room.Playlist.Add(new PlaylistItem
            {
                ID = Room.Playlist.Count,
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
    }
}
