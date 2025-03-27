// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public abstract partial class OnlinePlayBackgroundScreen : BackgroundScreen
    {
        private CancellationTokenSource? cancellationSource;
        private Background? lastBackground;
        private int? beatmapId;

        [BackgroundDependencyLoader]
        private void load()
        {
            loadNewBackground();
        }

        protected PlaylistItem? PlaylistItem
        {
            set
            {
                if (beatmapId == value?.Beatmap.OnlineID)
                    return;

                beatmapId = value?.Beatmap.OnlineID;

                if (LoadState >= LoadState.Ready)
                    loadNewBackground();
            }
        }

        private void loadNewBackground()
        {
            cancellationSource?.Cancel();
            cancellationSource = new CancellationTokenSource();

            if (beatmapId == null)
                switchBackground(new DefaultBackground());
            else
                LoadComponentAsync(new OnlineBeatmapBackground(beatmapId.Value), switchBackground, cancellationSource.Token);

            void switchBackground(Background newBackground)
            {
                float newDepth = 0;

                if (lastBackground != null)
                {
                    newDepth = lastBackground.Depth + 1;
                    lastBackground.FinishTransforms();
                    lastBackground.FadeOut(250);
                    lastBackground.Expire();
                }

                newBackground.Depth = newDepth;
                newBackground.Colour = ColourInfo.GradientVertical(new Color4(0.1f, 0.1f, 0.1f, 1f), new Color4(0.4f, 0.4f, 0.4f, 1f));
                newBackground.BlurTo(new Vector2(10));

                AddInternal(lastBackground = newBackground);
            }
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);
            this.MoveToX(0, TRANSITION_LENGTH);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            bool result = base.OnExiting(e);
            this.MoveToX(0);
            return result;
        }

        [LongRunningLoad]
        private partial class OnlineBeatmapBackground : Background
        {
            private readonly int beatmapId;

            public OnlineBeatmapBackground(int beatmapId)
            {
                this.beatmapId = beatmapId;
            }

            [BackgroundDependencyLoader]
            private void load(BeatmapLookupCache lookupCache, LargeTextureStore textures, CancellationToken cancellationToken)
            {
                try
                {
                    APIBeatmap? beatmap = lookupCache.GetBeatmapAsync(beatmapId, cancellationToken).GetResultSafely();
                    string? coverImage = beatmap?.BeatmapSet?.Covers.Cover;

                    if (coverImage != null)
                        Sprite.Texture = textures.Get(coverImage);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Failed to retrieve cover image for beatmap {beatmapId}.");
                }
            }
        }

        private partial class DefaultBackground : Background
        {
            [Resolved]
            private BeatmapManager beatmapManager { get; set; } = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                Sprite.Texture = beatmapManager.DefaultBeatmap.GetBackground();
            }
        }
    }
}
