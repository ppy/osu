// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osuTK;
using Realms;

namespace osu.Game.Screens.Import
{
    [Cached(typeof(IPreviewTrackOwner))]
    public partial class ReplayMissingBeatmapScreen : OsuScreen, IPreviewTrackOwner
    {
        [Resolved]
        private BeatmapModelDownloader beatmapDownloader { get; set; } = null!;

        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private IDisposable? realmSubscription;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        [Resolved]
        private INotificationOverlay? notificationOverlay { get; set; }

        private Container beatmapPanelContainer = null!;
        private ReplayDownloadButton replayDownloadButton = null!;
        private SettingsCheckbox automaticDownload = null!;

        private readonly MemoryStream scoreStream;

        private readonly APIBeatmapSet beatmapSetInfo;

        public ReplayMissingBeatmapScreen(APIBeatmap beatmap, MemoryStream scoreStream)
        {
            beatmapSetInfo = beatmap.BeatmapSet!;

            this.scoreStream = scoreStream;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OsuConfigManager config)
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Masking = true,
                    CornerRadius = 20,
                    AutoSizeAxes = Axes.Both,
                    AutoSizeDuration = 500,
                    AutoSizeEasing = Easing.OutQuint,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colours.Gray5,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new FillFlowContainer
                        {
                            Margin = new MarginPadding(20),
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Spacing = new Vector2(15),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = "Beatmap info",
                                    Font = OsuFont.Default.With(size: 30),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Spacing = new Vector2(15),
                                    Children = new Drawable[]
                                    {
                                        beatmapPanelContainer = new Container
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                        },
                                    }
                                },
                                automaticDownload = new SettingsCheckbox
                                {
                                    LabelText = "Automatically download beatmaps",
                                    Current = config.GetBindable<bool>(OsuSetting.AutomaticallyDownloadWhenSpectating),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                },
                                replayDownloadButton = new ReplayDownloadButton(new ScoreInfo())
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                },
                            }
                        }
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateStatus();
            realmSubscription = realm.RegisterForNotifications(
                realm => realm.All<BeatmapSetInfo>().Where(s => !s.DeletePending), beatmapsChanged);
        }

        private void updateStatus()
        {
            if (beatmapSetInfo == null) return;

            beatmapPanelContainer.Clear();
            beatmapPanelContainer.Child = new BeatmapCardNormal(beatmapSetInfo, allowExpansion: false);
            checkForAutomaticDownload(beatmapSetInfo);
        }

        private void checkForAutomaticDownload(APIBeatmapSet beatmap)
        {
            if (!automaticDownload.Current.Value)
                return;

            beatmapDownloader.Download(beatmap);
        }

        private void beatmapsChanged(IRealmCollection<BeatmapSetInfo> sender, ChangeSet? changes)
        {
            if (changes?.InsertedIndices == null) return;

            if (!scoreStream.CanRead) return;

            if (sender.Any(b => b.OnlineID == beatmapSetInfo.OnlineID))
            {
                var progressNotification = new ImportProgressNotification();
                var importTask = new ImportTask(scoreStream, "score.osr");
                scoreManager.Import(progressNotification, new[] { importTask })
                            .ContinueWith(s =>
                            {
                                s.GetResultSafely<IEnumerable<Live<ScoreInfo>>>().FirstOrDefault()?.PerformRead(score =>
                                {
                                    Guid scoreid = score.ID;
                                    Scheduler.Add(() =>
                                    {
                                        replayDownloadButton.Score.Value = realm.Realm.Find<ScoreInfo>(scoreid) ?? new ScoreInfo();
                                    });
                                });
                            });

                notificationOverlay?.Post(progressNotification);

                realmSubscription?.Dispose();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            realmSubscription?.Dispose();
        }
    }
}
