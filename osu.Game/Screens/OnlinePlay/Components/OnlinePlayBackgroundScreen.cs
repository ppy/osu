// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Online.Rooms;
using osuTK;
using osuTK.Graphics;

#nullable enable

namespace osu.Game.Screens.OnlinePlay.Components
{
    public abstract class OnlinePlayBackgroundScreen : BackgroundScreen
    {
        private CancellationTokenSource? cancellationSource;
        private PlaylistItemBackground? background;

        protected OnlinePlayBackgroundScreen()
            : base(false)
        {
            AddInternal(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Depth = float.MinValue,
                Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.9f), Color4.Black.Opacity(0.6f))
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            switchBackground(new PlaylistItemBackground(playlistItem));
        }

        private PlaylistItem? playlistItem;

        protected PlaylistItem? PlaylistItem
        {
            get => playlistItem;
            set
            {
                if (playlistItem == value)
                    return;

                playlistItem = value;

                if (LoadState > LoadState.Ready)
                    updateBackground();
            }
        }

        private void updateBackground()
        {
            Schedule(() =>
            {
                var beatmap = playlistItem?.Beatmap.Value;

                string? lastCover = (background?.Beatmap?.BeatmapSet as IBeatmapSetOnlineInfo)?.Covers.Cover;
                string? newCover = (beatmap?.BeatmapSet as IBeatmapSetOnlineInfo)?.Covers.Cover;

                if (lastCover == newCover)
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

        public override void OnSuspending(IScreen next)
        {
            base.OnSuspending(next);
            this.MoveToX(0, TRANSITION_LENGTH);
        }

        public override bool OnExiting(IScreen next)
        {
            bool result = base.OnExiting(next);
            this.MoveToX(0);
            return result;
        }
    }
}
