// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
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
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Scoring;
using osuTK.Graphics;
using Realms;

namespace osu.Game.Database
{
    public partial class MissingBeatmapNotification : ProgressNotification
    {
        [Resolved]
        private BeatmapModelDownloader beatmapDownloader { get; set; } = null!;

        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private readonly MemoryStream scoreStream;
        private readonly APIBeatmapSet beatmapSetInfo;
        private readonly string beatmapHash;

        private Bindable<bool> autodownloadConfig = null!;
        private Bindable<bool> noVideoSetting = null!;

        private IDisposable? realmSubscription;

        public MissingBeatmapNotification(APIBeatmap beatmap, MemoryStream scoreStream, string beatmapHash)
        {
            beatmapSetInfo = beatmap.BeatmapSet!;

            this.beatmapHash = beatmapHash;
            this.scoreStream = scoreStream;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, BeatmapSetOverlay? beatmapSetOverlay)
        {
            Text = "You do not have the required beatmap for this replay";

            realmSubscription = realm.RegisterForNotifications(
                realm => realm.All<BeatmapSetInfo>().Where(s => !s.DeletePending), beatmapsChanged);

            realm.Run(r =>
            {
                if (r.All<BeatmapSetInfo>().Any(s => !s.DeletePending && s.OnlineID == beatmapSetInfo.OnlineID))
                {
                    Text = "You have the corresponding beatmapset but no beatmap, you may need to update the beatmapset.";
                }
            });

            autodownloadConfig = config.GetBindable<bool>(OsuSetting.AutomaticallyDownloadWhenSpectating);
            noVideoSetting = config.GetBindable<bool>(OsuSetting.PreferNoVideo);

            Content.Add(new ClickableContainer
            {
                RelativeSizeAxes = Axes.X,
                Height = 70,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.TopLeft,
                CornerRadius = 4,
                Masking = true,
                Action = () => beatmapSetOverlay?.FetchAndShowBeatmapSet(beatmapSetInfo.OnlineID),
                Children = new Drawable[]
                {
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
                    new BeatmapDownloadButton(beatmapSetInfo)
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Width = 50,
                        Height = 30,
                        Margin = new MarginPadding
                        {
                            Bottom = 1f
                        }
                    }
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (autodownloadConfig.Value)
                beatmapDownloader.Download(beatmapSetInfo, noVideoSetting.Value);
        }

        private void beatmapsChanged(IRealmCollection<BeatmapSetInfo> sender, ChangeSet? changes)
        {
            if (changes?.InsertedIndices == null) return;

            if (sender.Any(s => s.Beatmaps.Any(b => b.MD5Hash == beatmapHash)))
            {
                var importTask = new ImportTask(scoreStream, "score.osr");
                scoreManager.Import(this, new[] { importTask });
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
