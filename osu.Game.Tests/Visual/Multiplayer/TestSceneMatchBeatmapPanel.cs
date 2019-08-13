// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Match.Components;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Game.Audio;
using osu.Framework.Allocation;

namespace osu.Game.Tests.Visual.Multiplayer
{
    [Cached(typeof(IPreviewTrackOwner))]
    public class TestSceneMatchBeatmapPanel : MultiplayerTestScene, IPreviewTrackOwner
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(MatchBeatmapPanel)
        };

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; }

        public TestSceneMatchBeatmapPanel()
        {
            Add(new MatchBeatmapPanel
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            Room.Playlist.Add(new PlaylistItem { Beatmap = new BeatmapInfo { OnlineBeatmapID = 1763072 } });
            Room.Playlist.Add(new PlaylistItem { Beatmap = new BeatmapInfo { OnlineBeatmapID = 2101557 } });
            Room.Playlist.Add(new PlaylistItem { Beatmap = new BeatmapInfo { OnlineBeatmapID = 1973466 } });
            Room.Playlist.Add(new PlaylistItem { Beatmap = new BeatmapInfo { OnlineBeatmapID = 2109801 } });
            Room.Playlist.Add(new PlaylistItem { Beatmap = new BeatmapInfo { OnlineBeatmapID = 1922035 } });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("Select random beatmap", () =>
            {
                Room.CurrentItem.Value = Room.Playlist[RNG.Next(Room.Playlist.Count)];
                previewTrackManager.StopAnyPlaying(this);
            });
        }
    }
}
