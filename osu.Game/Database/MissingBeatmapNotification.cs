// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Scoring;
using osuTK.Graphics;

namespace osu.Game.Database
{
    public partial class MissingBeatmapNotification : ProgressNotification
    {
        [Resolved]
        private BeatmapModelDownloader beatmapDownloader { get; set; } = null!;

        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        [Resolved]
        private BeatmapSetOverlay? beatmapSetOverlay { get; set; }

        private Container beatmapPanelContainer = null!;

        private readonly MemoryStream scoreStream;

        private readonly APIBeatmapSet beatmapSetInfo;

        private BeatmapDownloadTracker? downloadTracker;

        private Bindable<bool> autodownloadConfig = null!;

        public MissingBeatmapNotification(APIBeatmap beatmap, MemoryStream scoreStream)
        {
            beatmapSetInfo = beatmap.BeatmapSet!;

            this.scoreStream = scoreStream;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OsuConfigManager config)
        {
            autodownloadConfig = config.GetBindable<bool>(OsuSetting.AutomaticallyDownloadWhenSpectating);

            Text = "You do not have the required beatmap for this replay";

            Content.Add(beatmapPanelContainer = new ClickableContainer
            {
                RelativeSizeAxes = Axes.X,
                Height = 70,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.TopLeft,
                Action = () => beatmapSetOverlay?.FetchAndShowBeatmapSet(beatmapSetInfo.OnlineID)
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            downloadTracker = new BeatmapDownloadTracker(beatmapSetInfo);
            downloadTracker.State.BindValueChanged(downloadStatusChanged, true);

            beatmapPanelContainer.Clear();
            beatmapPanelContainer.Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 4,
                Children = new Drawable[]
                {
                    downloadTracker,
                    new DelayedLoadWrapper(() => new UpdateableOnlineBeatmapSetCover(BeatmapSetCoverType.Card)
                    {
                        OnlineInfo = beatmapSetInfo,
                        RelativeSizeAxes = Axes.Both
                    })
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.4f
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding
                        {
                            Left = 10f,
                            Top = 5f
                        },
                        Children = new Drawable[]
                        {
                            new TruncatingSpriteText
                            {
                                Text = beatmapSetInfo.Title,
                                Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 17, italics: true),
                                RelativeSizeAxes = Axes.X,
                            },
                            new TruncatingSpriteText
                            {
                                Text = beatmapSetInfo.Artist,
                                Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 12, italics: true),
                                RelativeSizeAxes = Axes.X,
                            }
                        }
                    },
                    new DownloadButton
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Width = 50,
                        Height = 30,
                        Margin = new MarginPadding
                        {
                            Bottom = 1f
                        },
                        Action = () => beatmapDownloader.Download(beatmapSetInfo),
                        State = { BindTarget = downloadTracker.State }
                    }
                }
            };

            if (autodownloadConfig.Value)
                beatmapDownloader.Download(beatmapSetInfo);
        }

        private void downloadStatusChanged(ValueChangedEvent<DownloadState> status)
        {
            if (status.NewValue != DownloadState.LocallyAvailable)
                return;

            var importTask = new ImportTask(scoreStream, "score.osr");
            scoreManager.Import(this, new[] { importTask });
        }
    }
}
