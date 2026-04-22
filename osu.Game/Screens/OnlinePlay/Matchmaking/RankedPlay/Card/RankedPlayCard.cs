// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Database;
using osu.Game.Online.Rooms;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card
{
    [Cached]
    public partial class RankedPlayCard : CompositeDrawable
    {
        public static readonly Vector2 SIZE = new Vector2(120, 200);

        public static readonly float CORNER_RADIUS = 6;

        public readonly RankedPlayCardWithPlaylistItem Item;

        private readonly IBindable<MultiplayerPlaylistItem?> playlistItem;

        public readonly Bindable<bool> SongPreviewEnabled = new BindableBool(true);

        private readonly Container content;
        private readonly Container cardContent;
        private readonly Container shadow;
        private readonly SelectionOutline selectionOutline;
        private readonly SongPreviewContainer songPreviewContainer;

        public bool ShowSelectionOutline
        {
            set => selectionOutline.FadeTo(value ? 1 : 0, 50);
        }

        public bool PlayAudioPreview
        {
            set => songPreviewContainer.CardHovered.Value = value;
        }

        public float Elevation;

        public bool PreviewTrackLoaded => songPreviewContainer.TrackLoaded;
        public bool PreviewTrackRunning => songPreviewContainer.IsRunning;

        private Sample? cardFlipSample;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        public RankedPlayCard(RankedPlayCardWithPlaylistItem item)
        {
            Item = item;

            Size = SIZE;

            playlistItem = item.PlaylistItem.GetBoundCopy();

            InternalChild = songPreviewContainer = new SongPreviewContainer
            {
                Enabled = { BindTarget = SongPreviewEnabled },
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    shadow = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = CORNER_RADIUS,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow,
                            Radius = 5,
                            Colour = Color4.Black.Opacity(0.1f),
                        },
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true,
                        }
                    },
                    content = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children =
                        [
                            new RankedPlayCardBackSide(),
                            cardContent = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Child = Empty(),
                            },
                            selectionOutline = new SelectionOutline
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                            }
                        ]
                    }
                ]
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            cardFlipSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/card-flip-1");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            playlistItem.BindValueChanged(e => onPlaylistItemChanged(e.NewValue), true);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            shadow.Scale = content.Scale;
            shadow.Size = new Vector2(1 - Elevation * 0.25f);
            shadow.Position = new Vector2(-25, 20) * Elevation;
        }

        #region beatmap fetching logic & card flip

        private readonly TaskCompletionSource cardRevealed = new TaskCompletionSource();

        public Task CardRevealed => cardRevealed.Task;

        private void onPlaylistItemChanged(MultiplayerPlaylistItem? playlistItem)
        {
            if (playlistItem == null)
            {
                SetContent(null);
                return;
            }

            loadCardContentAsync(playlistItem);
        }

        private void loadCardContentAsync(MultiplayerPlaylistItem playlistItem) => Task.Run(async () =>
        {
            var beatmap = await beatmapLookupCache.GetBeatmapAsync(playlistItem.BeatmapID).ConfigureAwait(false);

            cardRevealed.TrySetResult();

            if (beatmap == null)
            {
                Logger.Log($"Failed to load beatmap {playlistItem.BeatmapID} for playlistItem {playlistItem.ID}.", level: LogLevel.Error);
                return;
            }

            Schedule(() =>
            {
                SetContent(new RankedPlayCardContent(beatmap));
                songPreviewContainer.LoadPreview(beatmap);
            });
        });

        private bool hasContent;

        public void SetContent(Drawable? newContent)
        {
            if (newContent == null && !hasContent)
                return;

            hasContent = newContent != null;
            content.ScaleTo(new Vector2(0, 1), 100, Easing.In)
                   .Then()
                   .Schedule(() => cardContent.Child = newContent ?? Empty())
                   .ScaleTo(new Vector2(1), 300, Easing.OutElasticQuarter);

            SamplePlaybackHelper.PlayWithRandomPitch(cardFlipSample);
        }

        #endregion

        public void PopOutAndExpire()
        {
            content.ScaleTo(0, 500, Easing.In);

            this.FadeOut(500)
                .Expire();
        }

        private partial class SelectionOutline : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                const float border_width = 4;

                InternalChild = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    // anti-aliasing would create a gap between the border & card here if we used border_width directly
                    Padding = new MarginPadding(-(border_width - 1)),
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = CORNER_RADIUS + border_width,
                        BorderThickness = border_width,
                        BorderColour = Color4Extensions.FromHex("72D5FF"),
                        Blending = BlendingParameters.Additive,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Glow,
                            Radius = 30,
                            Colour = Color4Extensions.FromHex("72D5FF").Opacity(0.2f),
                            Hollow = true,
                            Roundness = 10
                        },
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    }
                };
            }
        }
    }
}
