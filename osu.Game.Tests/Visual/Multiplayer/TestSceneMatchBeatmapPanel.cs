// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Match.Components;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMatchBeatmapPanel : MultiplayerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(MatchBeatmapPanel)
        };

        public TestSceneMatchBeatmapPanel()
        {
            Add(new MatchBeatmapPanel
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            var playlist = Room.Playlist;

            playlist.Add(new PlaylistItem { Beatmap = new BeatmapInfo { OnlineBeatmapID = 1763072 } });
            playlist.Add(new PlaylistItem { Beatmap = new BeatmapInfo { OnlineBeatmapID = 2101557 } });
            playlist.Add(new PlaylistItem { Beatmap = new BeatmapInfo { OnlineBeatmapID = 1973466 } });
            playlist.Add(new PlaylistItem { Beatmap = new BeatmapInfo { OnlineBeatmapID = 2109801 } });
            playlist.Add(new PlaylistItem { Beatmap = new BeatmapInfo { OnlineBeatmapID = 1922035 } });

            AddStep("Select random beatmap", () => Room.CurrentItem.Value = playlist[RNG.Next(playlist.Count)]);
        }
    }
}
