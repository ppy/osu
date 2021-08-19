// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Linq;
using System.Threading;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.Rooms;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public class RoomBackgroundScreen : BackgroundScreen
    {
        private CancellationTokenSource? cancellationSource;
        private PlaylistItemBackground? background;

        private readonly BindableList<PlaylistItem> playlist = new BindableList<PlaylistItem>();

        public RoomBackgroundScreen()
        {
            playlist.BindCollectionChanged((_, __) => updateBackground());
        }

        private Room? room;

        public Room? Room
        {
            get => room;
            set
            {
                if (room == value)
                    return;

                if (room != null)
                    playlist.UnbindFrom(room.Playlist);

                room = value;

                if (room != null)
                    playlist.BindTo(room.Playlist);
                else
                    playlist.Clear();
            }
        }

        private void updateBackground()
        {
            Schedule(() =>
            {
                var playlistItem = playlist.FirstOrDefault();
                var beatmap = playlistItem?.Beatmap.Value;

                if (background?.BeatmapInfo?.BeatmapSet?.OnlineInfo?.Covers?.Cover == beatmap?.BeatmapSet?.OnlineInfo?.Covers?.Cover)
                    return;

                cancellationSource?.Cancel();
                LoadComponentAsync(new PlaylistItemBackground(playlistItem), switchBackground, (cancellationSource = new CancellationTokenSource()).Token);
            });
        }

        private void switchBackground(PlaylistItemBackground newBackground)
        {
            float newDepth = 0;

            if (background != null)
            {
                newDepth = background.Depth + 1;
                background.FinishTransforms();
                background.FadeOut(250);
                background.Expire();
            }

            newBackground.Depth = newDepth;
            newBackground.BlurTo(new Vector2(10));

            AddInternal(background = newBackground);
        }
    }
}
