// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Screens.Select;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCasePlaylist : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(BeatmapPlaylist),
            typeof(BeatmapPlaylistItem),
        };

        private BeatmapPlaylist playlist;

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            Add(playlist = new BeatmapPlaylist());

            var beatmap = new TestBeatmap(rulesets.GetRuleset(0));

            var playlistItem = new PlaylistItem
            {
                Beatmap = beatmap.BeatmapInfo,
                Ruleset = beatmap.BeatmapInfo.Ruleset,
                RulesetID = Ruleset.Value.ID ?? 0
            };

            for (int i = 0; i < 3; i++)
            {
                playlist.AddItem(playlistItem);
            }

            AddStep("AddItem", () =>
            {
                playlist.AddItem(playlistItem);
            });
        }
    }
}
