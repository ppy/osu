// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Screens.Multi.Components;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMatchSongSelect : MultiplayerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(MatchSongSelect),
            typeof(MatchBeatmapDetailArea),
        };

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Room.Playlist.Clear();
        });

        [Test]
        public void TestLoadSongSelect()
        {
            AddStep("create song select", () => LoadScreen(new MatchSongSelect()));
        }
    }
}
