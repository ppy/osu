// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Configuration;
using osu.Game.IO.Archives;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Notifications;
using osu.Game.Scoring;
using Realms;

namespace osu.Game.Database
{
    public partial class MissingBeatmapNotification : SimpleNotification
    {
        [Resolved]
        private BeatmapModelDownloader beatmapDownloader { get; set; } = null!;

        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private readonly ArchiveReader scoreArchive;
        private readonly APIBeatmapSet beatmapSetInfo;
        private readonly string beatmapHash;

        private Bindable<bool> autoDownloadConfig = null!;
        private Bindable<bool> noVideoSetting = null!;
        private BeatmapCardNano card = null!;

        private IDisposable? realmSubscription;

        public MissingBeatmapNotification(APIBeatmap beatmap, ArchiveReader scoreArchive, string beatmapHash)
        {
            beatmapSetInfo = beatmap.BeatmapSet!;

            this.beatmapHash = beatmapHash;
            this.scoreArchive = scoreArchive;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            realmSubscription = realm.RegisterForNotifications(
                realm => realm.All<BeatmapSetInfo>().Where(s => !s.DeletePending), beatmapsChanged);

            autoDownloadConfig = config.GetBindable<bool>(OsuSetting.AutomaticallyDownloadMissingBeatmaps);
            noVideoSetting = config.GetBindable<bool>(OsuSetting.PreferNoVideo);

            Content.Add(card = new BeatmapCardNano(beatmapSetInfo));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (autoDownloadConfig.Value)
            {
                Text = NotificationsStrings.DownloadingBeatmapForReplay;
                beatmapDownloader.Download(beatmapSetInfo, noVideoSetting.Value);
            }
            else
            {
                bool missingSetMatchesExistingOnlineId = realm.Run(r => r.All<BeatmapSetInfo>().Any(s => !s.DeletePending && s.OnlineID == beatmapSetInfo.OnlineID));
                Text = missingSetMatchesExistingOnlineId ? NotificationsStrings.MismatchingBeatmapForReplay : NotificationsStrings.MissingBeatmapForReplay;
            }
        }

        protected override void Update()
        {
            base.Update();
            card.Width = Content.DrawWidth;
        }

        private void beatmapsChanged(IRealmCollection<BeatmapSetInfo> sender, ChangeSet? changes)
        {
            if (changes?.InsertedIndices == null) return;

            if (sender.Any(s => s.Beatmaps.Any(b => b.MD5Hash == beatmapHash)))
            {
                string name = scoreArchive.Filenames.First(f => f.EndsWith(".osr", StringComparison.OrdinalIgnoreCase));
                var importTask = new ImportTask(scoreArchive.GetStream(name), name);
                scoreManager.Import(new[] { importTask });
                realmSubscription?.Dispose();
                Close(false);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            realmSubscription?.Dispose();
        }
    }
}
